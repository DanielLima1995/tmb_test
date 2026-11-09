using Loja.Application.DTOs;
using Loja.Application.DTOs.Insert;
using Loja.Application.Interfaces;
using Loja.Domain.Entities;
using Loja.Domain.Interfaces;
using Mapster;

namespace Loja.Application.Services;

public class HistoricoStatusService : IHistoricoStatusService
{
    private readonly IHistoricoStatusRepository _historicoStatusRepository;

    public HistoricoStatusService(IHistoricoStatusRepository historicoStatusRepository)
    {
        _historicoStatusRepository = historicoStatusRepository;
    }

    public async Task<List<HistoricoStatusDTO>> GetHistoricoStatus(int id)
    {
        var historicoStatus = await _historicoStatusRepository.GetHistoricoStatusAsync(id);
        var historicoStatusDto = historicoStatus.Adapt<List<HistoricoStatusDTO>>();
        return historicoStatusDto;
    }

    public async Task<int> Add(HistoricoStatusInsertDTO historicoStatusInsertDTO)
    {
        var historicoStatus = new HistoricoStatus(
            historicoStatusInsertDTO.PedidoId,
            historicoStatusInsertDTO.Status,
            DateTime.Now
        );

        return await _historicoStatusRepository.AddAsync(historicoStatus);
    }

}