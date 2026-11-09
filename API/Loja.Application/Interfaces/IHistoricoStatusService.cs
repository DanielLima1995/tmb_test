using Loja.Application.DTOs;
using Loja.Application.DTOs.Insert;
using Loja.Application.DTOs.Pagination;
using Loja.Application.DTOs.Update;

namespace Loja.Application.Interfaces;

public interface IHistoricoStatusService
{
    Task<List<HistoricoStatusDTO>> GetHistoricoStatus(int id);
    Task<int> Add(HistoricoStatusInsertDTO historicoStatusInsertDto);
}
