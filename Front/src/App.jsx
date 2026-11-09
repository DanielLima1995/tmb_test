import React, { useCallback, useEffect, useState } from "react";
import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { createOrder, getOrder, getOrders } from "./api.js";

const PER_PAGE = 10;
const pedidosHubUrl = import.meta.env.VITE_PEDIDOS_HUB_URL || "/hubs/pedidos";
const currency = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});
const dateTime = new Intl.DateTimeFormat("pt-BR", {
  dateStyle: "medium",
  timeStyle: "short",
});

function formatDate(value) {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : dateTime.format(date);
}

function StatusBadge({ status }) {
  const normalized = (status || "Pendente").toLowerCase();
  const tone = normalized.includes("entreg") || normalized.includes("conclu")
    ? "success"
    : normalized.includes("cancel")
      ? "danger"
      : normalized.includes("andamento") || normalized.includes("process")
        ? "info"
        : "pending";

  return <span className={`status-badge ${tone}`}>{status || "Pendente"}</span>;
}

function Modal({ title, onClose, children }) {
  return (
    <div className="modal-backdrop" onMouseDown={(event) => {
      if (event.target === event.currentTarget) onClose();
    }}>
      <section className="modal-card" role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <div className="modal-heading">
          <div>
            <p className="eyebrow">PAINEL DE PEDIDOS</p>
            <h2 id="modal-title">{title}</h2>
          </div>
          <button className="icon-button" type="button" onClick={onClose} aria-label="Fechar">
            ×
          </button>
        </div>
        {children}
      </section>
    </div>
  );
}

function App() {
  const [orders, setOrders] = useState([]);
  const [page, setPage] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [loading, setLoading] = useState(true);
  const [listError, setListError] = useState("");
  const [modal, setModal] = useState("");
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState("");
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState("");
  const [form, setForm] = useState({ cliente: "", produto: "", valor: "" });
  const [statusToast, setStatusToast] = useState(null);

  const loadOrders = useCallback(async () => {
    setLoading(true);
    setListError("");
    try {
      const result = await getOrders(page, PER_PAGE);
      if (!result || !Array.isArray(result.items)) {
        throw new Error("A resposta da API não contém uma lista de pedidos válida.");
      }
      setOrders(result.items);
      setTotalCount(Number(result.totalCount) || 0);
      setTotalPages(Number(result.totalPages) || 0);
    } catch (error) {
      setListError(error.message || "Não foi possível carregar os pedidos.");
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  useEffect(() => {
    if (!statusToast) return undefined;
    const timeout = window.setTimeout(() => setStatusToast(null), 5000);
    return () => window.clearTimeout(timeout);
  }, [statusToast]);

  useEffect(() => {
    let disposed = false;
    let retryTimer;
    const connection = new HubConnectionBuilder()
      .withUrl(pedidosHubUrl)
      .withAutomaticReconnect()
      .build();

    const handleStatusUpdate = (evento) => {
      if (evento?.pedidoId == null || typeof evento.status !== "string") {
        console.error("O evento StatusPedidoAtualizado recebeu dados inválidos.", evento);
        return;
      }

      const dataAlteracao = evento.dataAlteracao || new Date().toISOString();
      const pedidoId = String(evento.pedidoId);
      setStatusToast((current) => ({
        id: (current?.id || 0) + 1,
        message: `Pedido #${pedidoId.padStart(4, "0")} atualizado para "${evento.status}".`,
      }));
      setOrders((current) => current.map((order) => (
        String(order.id) === pedidoId ? { ...order, status: evento.status } : order
      )));
      setSelectedOrder((current) => {
        if (!current || String(current.id) !== pedidoId) return current;

        const historicoStatus = Array.isArray(current.historicoStatus)
          ? current.historicoStatus
          : [];
        const alreadyRecorded = historicoStatus.some((item) => (
          item.status === evento.status && item.dataAlteracao === dataAlteracao
        ));

        return {
          ...current,
          status: evento.status,
          historicoStatus: alreadyRecorded
            ? historicoStatus
            : [
              ...historicoStatus,
              {
                id: `realtime-${pedidoId}-${dataAlteracao}`,
                status: evento.status,
                dataAlteracao,
              },
            ],
        };
      });
    };

    const scheduleReconnect = () => {
      if (disposed || retryTimer !== undefined) return;
      retryTimer = window.setTimeout(() => {
        retryTimer = undefined;
        void startConnection();
      }, 5000);
    };

    const startConnection = async () => {
      if (disposed || connection.state !== HubConnectionState.Disconnected) return;
      try {
        await connection.start();
      } catch (error) {
        console.error("Não foi possível conectar à mensageria de pedidos.", error);
        scheduleReconnect();
      }
    };

    connection.on("StatusPedidoAtualizado", handleStatusUpdate);
    connection.onreconnecting((error) => {
      console.warn("Conexão com a mensageria de pedidos perdida; tentando reconectar.", error);
    });
    connection.onreconnected(() => {
      console.info("Conexão com a mensageria de pedidos restabelecida.");
    });
    connection.onclose((error) => {
      if (error) console.error("A conexão com a mensageria de pedidos foi encerrada.", error);
      scheduleReconnect();
    });
    void startConnection();

    return () => {
      disposed = true;
      if (retryTimer !== undefined) window.clearTimeout(retryTimer);
      connection.off("StatusPedidoAtualizado", handleStatusUpdate);
      void connection.stop().catch((error) => {
        console.error("Não foi possível encerrar a conexão da mensageria de pedidos.", error);
      });
    };
  }, []);

  async function handleSubmit(event) {
    event.preventDefault();
    setSaving(true);
    setFormError("");
    try {
      await createOrder({
        cliente: form.cliente.trim(),
        produto: form.produto.trim(),
        valor: Number(form.valor),
      });
      setForm({ cliente: "", produto: "", valor: "" });
      setModal("");
      setPage(0);
      if (page === 0) await loadOrders();
    } catch (error) {
      setFormError(error.message || "Não foi possível criar o pedido.");
    } finally {
      setSaving(false);
    }
  }

  async function showDetails(order) {
    setModal("details");
    setSelectedOrder(null);
    setDetailError("");
    setDetailLoading(true);
    try {
      setSelectedOrder(await getOrder(order.id));
    } catch (error) {
      setDetailError(error.message || "Não foi possível carregar os detalhes do pedido.");
    } finally {
      setDetailLoading(false);
    }
  }

  const firstItem = totalCount === 0 ? 0 : page * PER_PAGE + 1;
  const lastItem = Math.min((page + 1) * PER_PAGE, totalCount);

  return (
    <main className="page-shell">
      <header className="topbar">
        <a className="brand" href="#" aria-label="Pedidos, início">
          <span className="brand-mark">P</span>
          <span>pedidos<span className="brand-dot">.</span></span>
        </a>
        <span className="topbar-label"><span className="online-dot" /> Visão geral</span>
      </header>

      <div className="content">
        <section className="welcome">
          <div>
            <p className="eyebrow">GESTÃO DE VENDAS</p>
            <h1>Seus pedidos,<br className="mobile-break" /> em um só lugar.</h1>
            <p className="welcome-copy">Acompanhe e organize cada pedido com praticidade.</p>
          </div>
          <button className="primary-button" onClick={() => {
            setFormError("");
            setModal("create");
          }}>
            <span className="plus">+</span> Novo pedido
          </button>
        </section>

        <section className="summary-card" aria-label="Resumo de pedidos">
          <div className="summary-icon" aria-hidden="true">▤</div>
          <div>
            <span className="summary-label">PEDIDOS CADASTRADOS</span>
            <strong className="summary-value">{loading ? "—" : totalCount}</strong>
          </div>
          <span className="summary-note">no total</span>
        </section>

        <section className="orders-card">
          <div className="table-heading">
            <div>
              <h2>Todos os pedidos</h2>
              <p>Consulte e acompanhe seus pedidos recentes.</p>
            </div>
            <span className="count-pill">{totalCount} {totalCount === 1 ? "pedido" : "pedidos"}</span>
          </div>

          {listError && (
            <div className="error-banner" role="alert">
              <span>{listError}</span>
              <button type="button" className="text-button" onClick={loadOrders}>Tentar novamente</button>
            </div>
          )}

          <div className="table-scroll">
            <table>
              <thead>
                <tr>
                  <th>Pedido</th>
                  <th>Cliente</th>
                  <th>Produto</th>
                  <th>Data</th>
                  <th>Valor</th>
                  <th>Status</th>
                  <th><span className="sr-only">Ações</span></th>
                </tr>
              </thead>
              <tbody>
                {loading ? (
                  <tr><td className="empty-state" colSpan="7">Carregando pedidos...</td></tr>
                ) : listError ? (
                  <tr><td className="empty-state" colSpan="7">Não foi possível exibir os pedidos.</td></tr>
                ) : orders.length === 0 ? (
                  <tr>
                    <td className="empty-state" colSpan="7">
                      <span className="empty-icon">✳</span>
                      <strong>Nenhum pedido por aqui</strong>
                      <span>Crie seu primeiro pedido para começar.</span>
                    </td>
                  </tr>
                ) : orders.map((order) => (
                  <tr key={order.id}>
                    <td className="order-id">#{String(order.id).padStart(4, "0")}</td>
                    <td className="customer-cell">{order.cliente}</td>
                    <td>{order.produto}</td>
                    <td className="muted-cell">{formatDate(order.dataCriacao)}</td>
                    <td className="value-cell">{currency.format(Number(order.valor) || 0)}</td>
                    <td><StatusBadge status={order.status} /></td>
                    <td>
                      <button className="details-button" type="button" onClick={() => showDetails(order)}>
                        Detalhes <span aria-hidden="true">↗</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <footer className="table-footer">
            <span>Exibindo <strong>{firstItem}–{lastItem}</strong> de <strong>{totalCount}</strong> pedidos</span>
            <div className="pagination">
              <button type="button" aria-label="Página anterior" disabled={page <= 0 || loading} onClick={() => setPage(page - 1)}>‹</button>
              <span>Página {page + 1}{totalPages > 0 ? ` de ${totalPages}` : ""}</span>
              <button type="button" aria-label="Próxima página" disabled={loading || totalPages === 0 || page + 1 >= totalPages} onClick={() => setPage(page + 1)}>›</button>
            </div>
          </footer>
        </section>

        <p className="page-footnote">Feito para deixar sua rotina mais leve <span>✦</span></p>
      </div>

      {statusToast && (
        <div key={statusToast.id} className="status-toast" role="status" aria-live="polite">
          <span className="status-toast-icon" aria-hidden="true">✓</span>
          <span>{statusToast.message}</span>
          <button type="button" onClick={() => setStatusToast(null)} aria-label="Fechar notificação">×</button>
        </div>
      )}

      {modal === "create" && (
        <Modal title="Criar novo pedido" onClose={() => setModal("")}>
          <form className="order-form" onSubmit={handleSubmit}>
            <p className="form-intro">Preencha as informações para adicionar um pedido à sua lista.</p>
            <label>
              Nome do cliente
              <input required autoFocus maxLength="120" placeholder="Ex.: Mariana Costa" value={form.cliente}
                onChange={(event) => setForm({ ...form, cliente: event.target.value })} />
            </label>
            <label>
              Produto
              <input required maxLength="160" placeholder="Ex.: Kit de escritório" value={form.produto}
                onChange={(event) => setForm({ ...form, produto: event.target.value })} />
            </label>
            <label>
              Valor do pedido
              <div className="currency-input">
                <span>R$</span>
                <input required type="number" min="0.01" step="0.01" placeholder="0,00" value={form.valor}
                  onChange={(event) => setForm({ ...form, valor: event.target.value })} />
              </div>
            </label>
            {formError && <p className="form-error" role="alert">{formError}</p>}
            <div className="modal-actions">
              <button type="button" className="secondary-button" onClick={() => setModal("")}>Cancelar</button>
              <button type="submit" className="primary-button" disabled={saving}>
                {saving ? "Salvando..." : "Criar pedido"}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {modal === "details" && (
        <Modal title="Detalhes do pedido" onClose={() => setModal("")}>
          {detailLoading ? <p className="detail-message">Carregando detalhes...</p> : detailError ? (
            <div className="detail-message">
              <p className="form-error" role="alert">{detailError}</p>
              <button type="button" className="secondary-button" onClick={() => setModal("")}>Fechar</button>
            </div>
          ) : selectedOrder && (
            <div className="detail-content">
              <div className="detail-order-number">Pedido #{String(selectedOrder.id).padStart(4, "0")}</div>
              <div className="detail-grid">
                <div><span>CLIENTE</span><strong>{selectedOrder.cliente}</strong></div>
                <div><span>PRODUTO</span><strong>{selectedOrder.produto}</strong></div>
                <div><span>VALOR</span><strong>{currency.format(Number(selectedOrder.valor) || 0)}</strong></div>
                <div><span>CRIADO EM</span><strong>{formatDate(selectedOrder.dataCriacao)}</strong></div>
                <div><span>STATUS ATUAL</span><StatusBadge status={selectedOrder.status} /></div>
              </div>
              <div className="history-section">
                <h3>Histórico de status</h3>
                {selectedOrder.historicoStatus?.length ? (
                  <ol className="timeline">
                    {selectedOrder.historicoStatus.map((item) => (
                      <li key={item.id}>
                        <span className="timeline-dot" />
                        <div><strong>{item.status}</strong><span>{formatDate(item.dataAlteracao)}</span></div>
                      </li>
                    ))}
                  </ol>
                ) : <p className="history-empty">Nenhuma alteração de status registrada.</p>}
              </div>
              <div className="modal-actions">
                <button type="button" className="secondary-button" onClick={() => setModal("")}>Fechar</button>
              </div>
            </div>
          )}
        </Modal>
      )}
    </main>
  );
}

export default App;
