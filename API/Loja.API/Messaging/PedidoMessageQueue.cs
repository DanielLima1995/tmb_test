using System.Threading.Channels;

namespace Loja.API.Messaging;

public sealed class PedidoMessageQueue : IPedidoMessageQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public ValueTask EnqueueAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(pedidoId, cancellationToken);
    }

    public IAsyncEnumerable<int> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
