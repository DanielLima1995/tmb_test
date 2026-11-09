using Loja.Domain.Validation;

namespace Loja.Domain.Entities;

public sealed class HistoricoStatus : Entity
{
    public HistoricoStatus(int pedidoId, string status, DateTime dataAlteracao)
    {
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "O status é obrigatório.");
        DomainExceptionValidation.When(dataAlteracao == default, "A data de alteração é obrigatória.");
        DomainExceptionValidation.When(status is not ("Pendente" or "Processando" or "Finalizado"), "O status é inválido.");

        PedidoId = pedidoId;
        Status = status;
        DataAlteracao = dataAlteracao;
        ValidateDomain(pedidoId, status, dataAlteracao);
    }
    public int PedidoId { get; private set; }
    public Pedidos? Pedido { get; private set; }

    public string Status { get; private set; }
    public DateTime DataAlteracao { get; private set; }


    private void ValidateDomain(int pedidoId, string status, DateTime dataAlteracao)
    {
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "O status é obrigatório.");
        DomainExceptionValidation.When(dataAlteracao == default, "A data de alteração é obrigatória.");
        DomainExceptionValidation.When(status is not ("Pendente" or "Processando" or "Finalizado"), "O status é inválido.");

        PedidoId = pedidoId;
        Status = status;
        DataAlteracao = dataAlteracao;
    }

}
