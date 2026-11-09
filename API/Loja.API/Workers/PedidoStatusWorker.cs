using Loja.API.Messaging;
using Loja.Application.Interfaces;

namespace Loja.API.Workers;

public sealed class PedidoStatusWorker : BackgroundService
{
    private readonly IPedidoMessageQueue _pedidoMessageQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PedidoStatusWorker> _logger;

    public PedidoStatusWorker(
        IPedidoMessageQueue pedidoMessageQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<PedidoStatusWorker> logger)
    {
        _pedidoMessageQueue = pedidoMessageQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var pedidoId in _pedidoMessageQueue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    var dataPendente = await GetPendingStatusTimeAsync(pedidoId);
                    if (dataPendente is null)
                    {
                        _logger.LogWarning(
                            "Pedido {PedidoId} não possui registro de status Pendente no histórico.",
                            pedidoId);
                        continue;
                    }

                    var tempoRestante = dataPendente.Value.AddSeconds(5) - DateTime.Now;
                    if (tempoRestante > TimeSpan.Zero)
                    {
                        await Task.Delay(tempoRestante, stoppingToken);
                    }

                    if (!await UpdateStatusAsync(pedidoId, "Processando"))
                    {
                        _logger.LogWarning("Pedido {PedidoId} não foi encontrado para processamento.", pedidoId);
                        continue;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                    if (!await UpdateStatusAsync(pedidoId, "Finalizado"))
                    {
                        _logger.LogWarning("Pedido {PedidoId} não foi encontrado para finalização.", pedidoId);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Erro ao processar o pedido {PedidoId}.", pedidoId);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("PedidoStatusWorker cancelado durante o shutdown.");
        }
    }

    private async Task<DateTime?> GetPendingStatusTimeAsync(int pedidoId)
    {
        using var scope = _scopeFactory.CreateScope();
        var historicoStatusService = scope.ServiceProvider.GetRequiredService<IHistoricoStatusService>();
        var historico = await historicoStatusService.GetHistoricoStatus(pedidoId);

        return historico
            .Where(item => item.Status == "Pendente")
            .OrderBy(item => item.DataAlteracao)
            .Select(item => (DateTime?)item.DataAlteracao)
            .FirstOrDefault();
    }

    private async Task<bool> UpdateStatusAsync(int pedidoId, string status)
    {
        using var scope = _scopeFactory.CreateScope();
        var pedidosService = scope.ServiceProvider.GetRequiredService<IPedidosService>();
        return await pedidosService.UpdateStatus(pedidoId, status);
    }
}
