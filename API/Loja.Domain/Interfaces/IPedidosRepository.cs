

using Loja.Domain.Entities;

namespace Loja.Domain.Interfaces;

public interface IPedidosRepository
{
    Task<List<Pedidos>> GetPedidosAsync();
    Task<Pedidos?> GetPedidoByIdAsync(int id);
    Task<int> AddAsync(Pedidos pedido);
    Task UpdateAsync(Pedidos pedido);
}
