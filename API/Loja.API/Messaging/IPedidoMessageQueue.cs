namespace Loja.API.Messaging;

public interface IPedidoMessageQueue
{
    ValueTask EnqueueAsync(int pedidoId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken);
}
