

using Loja.Domain.Entities;

namespace Loja.Domain.Interfaces;

public interface IHistoricoStatusRepository
{
    Task<List<HistoricoStatus>> GetHistoricoStatusAsync(int pedidoId);
    Task<int> AddAsync(HistoricoStatus historicoStatus);
}
