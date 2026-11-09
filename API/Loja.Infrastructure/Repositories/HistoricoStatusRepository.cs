using Loja.Domain.Entities;
using Loja.Domain.Interfaces;

using Loja.Infrastructure.Context;
using Loja.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Loja.Infrastructure.Repositories;

public class HistoricoStatusRepository : IHistoricoStatusRepository
{
    private readonly ApplicationDbContext _historicoStatusContext;

    public HistoricoStatusRepository(ApplicationDbContext context)
    {
        _historicoStatusContext = context;
    }

    public async Task<List<HistoricoStatus>> GetHistoricoStatusAsync(int pedidoId)
    {
        return await _historicoStatusContext.HistoricoStatus
            .AsNoTracking()
            .Where(hs => hs.PedidoId == pedidoId)
            .ToListAsync();
    }

    public async Task<int> AddAsync(HistoricoStatus historicoStatus)
    {
        _historicoStatusContext.HistoricoStatus.Add(historicoStatus);
        await _historicoStatusContext.SaveChangesAsync();
        return historicoStatus.Id;
    }
}