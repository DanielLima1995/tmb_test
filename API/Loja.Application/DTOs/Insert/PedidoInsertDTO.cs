namespace Loja.Application.DTOs.Insert;

public class PedidoInsertDTO
{
    public string Cliente { get; private set; } = string.Empty;
    public string Produto { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }

    public PedidoInsertDTO(string cliente, string produto, decimal valor)
    {
        Cliente = cliente;
        Produto = produto;
        Valor = valor;
    }
}