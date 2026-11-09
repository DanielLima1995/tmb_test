using Loja.API.Hubs;
using Loja.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Loja.API.Messaging;

public sealed class SignalRPedidoStatusNotifier : IPedidoStatusNotifier
{
    private readonly IHubContext<PedidosHub> _hubContext;

    public SignalRPedidoStatusNotifier(IHubContext<PedidosHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublicarStatusAlteradoAsync(
        int pedidoId,
        string status,
        DateTime dataAlteracao,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(
            "StatusPedidoAtualizado",
            new { pedidoId, status, dataAlteracao },
            cancellationToken);
    }
}
