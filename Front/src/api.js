const apiBaseUrl = "/api";

async function request(path, options = {}) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...options,
    headers: {
      ...(options.body ? { "Content-Type": "application/json" } : {}),
      ...options.headers,
    },
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Não foi possível concluir a solicitação (${response.status}).`);
  }

  if (response.status === 204) return null;

  const contentType = response.headers.get("content-type") || "";
  if (!contentType.includes("application/json") && !contentType.includes("+json")) {
    throw new Error(
      `A chamada ${path} recebeu conteúdo não JSON. Reinicie o servidor de desenvolvimento para carregar o proxy da API.`,
    );
  }

  return response.json();
}

export function getOrders(page, perPage) {
  const query = new URLSearchParams({ page: String(page), perPage: String(perPage) });
  return request(`/Pedidos?${query}`);
}

export function createOrder(order) {
  return request("/Pedidos", {
    method: "POST",
    body: JSON.stringify(order),
  });
}

export function getOrder(id) {
  return request(`/Pedidos/${encodeURIComponent(id)}`);
}
