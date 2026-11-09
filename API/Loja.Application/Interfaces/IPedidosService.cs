using Loja.Application.DTOs;
using Loja.Application.DTOs.Insert;
using Loja.Application.DTOs.Pagination;
using Loja.Application.DTOs.Update;

namespace Loja.Application.Interfaces;

public interface IPedidosService
{
    Task<PagedList<PedidosDTO>> GetPedidos();
    Task<PedidosDTO?> GetPedidoById(int id);
    Task<int> Add(PedidoInsertDTO pedidosInsertDto);
    Task<bool> UpdateStatus(int id, string status);
}
