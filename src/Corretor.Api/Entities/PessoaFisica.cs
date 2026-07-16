namespace Corretor.Api.Entities;

public sealed class PessoaFisica
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? FaixaEtaria { get; set; }
    public string? DataNascimento { get; set; }
    public string? NomeMae { get; set; }
    public string? NomePai { get; set; }
}
