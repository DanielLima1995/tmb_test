using Loja.Application.DTOs;
using Loja.Application.DTOs.Insert;
using Loja.Application.DTOs.Pagination;
using Loja.Application.DTOs.Update;
using Loja.Application.Interfaces;
using Loja.Domain.Entities;
using Loja.Domain.Interfaces;
using Loja.Domain.Parameters;
using Mapster;

namespace Loja.Application.Services;

public class PedidosService : IPedidosService
{
    private readonly IPedidosRepository _pedidosRepository;
    private readonly IPedidoStatusNotifier _pedidoStatusNotifier;

    public PedidosService(
        IPedidosRepository pedidosRepository,
        IPedidoStatusNotifier pedidoStatusNotifier)
    {
        _pedidosRepository = pedidosRepository;
        _pedidoStatusNotifier = pedidoStatusNotifier;
    }

    public async Task<PagedList<PedidosDTO>> GetPedidos()
    {
        var pedidos = await _pedidosRepository.GetPedidosAsync();
        var pedidosDto = pedidos.Adapt<List<PedidosDTO>>();
        var perPage = Math.Max(pedidosDto.Count, 1);

        return new PagedList<PedidosDTO>(pedidosDto, pedidosDto.Count, 1, perPage);
    }

    public async Task<PedidosDTO?> GetPedidoById(int id)
    {
        var pedido = await _pedidosRepository.GetPedidoByIdAsync(id);
        return pedido?.Adapt<PedidosDTO>();
    }

    public async Task<int> Add(PedidoInsertDTO pedidoInsertDTO)
    {
        var venda = new Pedidos(
            pedidoInsertDTO.Cliente,
            pedidoInsertDTO.Produto,
            pedidoInsertDTO.Valor,
            "Pendente",
            DateTime.Now
        );

        return await _pedidosRepository.AddAsync(venda);
    }

    public async Task<bool> UpdateStatus(int id, string status)
    {
        var pedido = await _pedidosRepository.GetPedidoByIdAsync(id);
        if (pedido is null)
        {
            return false;
        }

        pedido.InsertStatus(status);
        var historicoStatus = pedido.HistoricoStatus.Last();
        await _pedidosRepository.UpdateAsync(pedido);
        await _pedidoStatusNotifier.PublicarStatusAlteradoAsync(
            pedido.Id,
            historicoStatus.Status,
            historicoStatus.DataAlteracao);
        return true;
    }

}