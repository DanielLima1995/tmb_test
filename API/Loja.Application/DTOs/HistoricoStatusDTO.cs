namespace Loja.Application.DTOs;

public class HistoricoStatusDTO
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataAlteracao { get; set; }
}
