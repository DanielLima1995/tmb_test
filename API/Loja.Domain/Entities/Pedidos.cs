using Loja.Domain.Validation;
using System.Net.NetworkInformation;

namespace Loja.Domain.Entities;

public sealed class Pedidos: Entity
{

    public Pedidos(string cliente, string produto, decimal valor, string status, DateTime dataCriacao)
    {
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(cliente), "O cliente é obrigatório.");
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(produto), "O produto é obrigatório.");
        DomainExceptionValidation.When(valor < 0, "O valor da ordem não pode ser negativo.");
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "O status é obrigatório.");
        DomainExceptionValidation.When(dataCriacao == default, "A data de criação é obrigatória.");
        DomainExceptionValidation.When(status is not ("Pendente" or "Processando" or "Finalizado"), "O status é inválido.");
        Cliente = cliente;
        Produto = produto;
        Valor = valor;
        Status = status;
        DataCriacao = dataCriacao;

        ValidateDomain(cliente, produto, valor, status, dataCriacao);
    }

    public string Cliente { get; private set; }
    public string Produto { get; private set; }
    public decimal Valor { get; private set; }
    public string Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public ICollection<HistoricoStatus> HistoricoStatus { get; private set; } = new List<HistoricoStatus>();

    public void InsertStatus(string novoStatus)
    {
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(novoStatus), "O status é obrigatório.");
        DomainExceptionValidation.When(novoStatus is not ("Pendente" or "Processando" or "Finalizado"), "O status é inválido.");
        Status = novoStatus;
        HistoricoStatus.Add(new HistoricoStatus(Id, novoStatus, DateTime.Now));
    }

    public void Update(string cliente, string produto, decimal valor, string status)
    {
        ValidateDomain(cliente, produto, valor, status, DataCriacao);
    }

    private void ValidateDomain(string cliente, string produto, decimal valor, string status, DateTime dataCriacao)
    {
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(cliente), "O cliente é obrigatório.");
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(produto), "O produto é obrigatório.");
        DomainExceptionValidation.When(valor < 0, "O valor da ordem não pode ser negativo.");
        DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "O status é obrigatório.");
        DomainExceptionValidation.When(dataCriacao == default, "A data de criação é obrigatória.");

        Cliente = cliente;
        Produto = produto;
        Valor = valor;
        Status = status;
        DataCriacao = dataCriacao;
    }
}