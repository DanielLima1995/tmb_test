using Loja.API.Messaging;
using Loja.Application.DTOs;
using Loja.Application.DTOs.Insert;
using Loja.Application.DTOs.Pagination;
using Loja.Application.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Loja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IPedidosService _pedidosService;
    private readonly IHistoricoStatusService _historicoStatusService;
    private readonly IPedidoMessageQueue _pedidoMessageQueue;

    public PedidosController(
        IPedidosService pedidosService,
        IHistoricoStatusService historicoStatusService,
        IPedidoMessageQueue pedidoMessageQueue)
    {
        _pedidosService = pedidosService;
        _historicoStatusService = historicoStatusService;
        _pedidoMessageQueue = pedidoMessageQueue;
    }

    [HttpGet]
    public async Task<ActionResult<PagedList<PedidosDTO>>> Get()
    {
        var resultado = await _pedidosService.GetPedidos();
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidosDTO>> GetById(int id)
    {
        var pedido = await _pedidosService.GetPedidoById(id);
        if (pedido == null) return NotFound();
        return Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PedidoInsertDTO pedidosInsertDto)
    {

        int pedidoCadastrado = await _pedidosService.Add(pedidosInsertDto);

        if (pedidoCadastrado <= 0 || pedidosInsertDto is null)
        {
            return BadRequest(new { mensagem = "Erro ao adicionar o pedido." });
        }

        HistoricoStatusInsertDTO historicoStatusInsertDto = new HistoricoStatusInsertDTO
        {
            PedidoId = pedidoCadastrado,
            Status = "Pendente"
        };

        await _historicoStatusService.Add(historicoStatusInsertDto);
        await _pedidoMessageQueue.EnqueueAsync(pedidoCadastrado);
        return Ok(new { mensagem = "Pedido adicionado com sucesso.", pedidoId = pedidoCadastrado });
    }


}
