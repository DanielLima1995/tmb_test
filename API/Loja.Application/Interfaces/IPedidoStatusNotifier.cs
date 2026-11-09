namespace Loja.Application.Interfaces;

public interface IPedidoStatusNotifier
{
    Task PublicarStatusAlteradoAsync(
        int pedidoId,
        string status,
        DateTime dataAlteracao,
        CancellationToken cancellationToken = default);
}
