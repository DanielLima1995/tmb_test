using System.Net.NetworkInformation;

namespace Loja.Application.DTOs;

public class PedidosDTO
{
    public int Id { get; set; }
    public string Cliente { get; private set; } = string.Empty;
    public string Produto { get; private set; } = string.Empty;
    public decimal Valor { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }
    public List<HistoricoStatusDTO> HistoricoStatus { get; set; } = new();
}
