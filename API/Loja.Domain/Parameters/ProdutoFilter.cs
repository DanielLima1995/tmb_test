namespace Loja.Domain.Parameters;

public class ProdutoFilter
{
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 15;
    public string? Titulo { get; set; }
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
    public bool? Ativo { get; set; }
    public string? UsuarioId { get; set; }
    public string? Tags { get; set; }
}
