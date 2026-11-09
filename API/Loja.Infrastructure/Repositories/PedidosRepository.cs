using Loja.Domain.Entities;
using Loja.Domain.Interfaces;
using Loja.Domain.Parameters;
using Loja.Infrastructure.Context;
using Loja.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Loja.Infrastructure.Repositories;

public class PedidosRepository : IPedidosRepository
{
    private readonly ApplicationDbContext _pedidosContext;

    public PedidosRepository(ApplicationDbContext context)
    {
        _pedidosContext = context;
    }

    public async Task<List<Pedidos>> GetPedidosAsync()
    {
        return await _pedidosContext.Pedidos
            .Include(pedido => pedido.HistoricoStatus)
            .ToListAsync();
    }

    public async Task<Pedidos?> GetPedidoByIdAsync(int id)
    {
        return await _pedidosContext.Pedidos
            .Include(pedido => pedido.HistoricoStatus)
            .FirstOrDefaultAsync(pedido => pedido.Id == id);
    }

    public async Task<int> AddAsync(Pedidos pedido)
    {
        _pedidosContext.Pedidos.Add(pedido);
        await _pedidosContext.SaveChangesAsync();
        return pedido.Id;
    }

    public async Task UpdateAsync(Pedidos pedido)
    {
        await _pedidosContext.SaveChangesAsync();
    }

}