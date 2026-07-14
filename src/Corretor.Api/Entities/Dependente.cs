namespace Corretor.Api.Entities;

public sealed class Dependente
{
    public Guid Id { get; set; }
    public Guid PessoaFisicaId { get; set; }
    public string? DataNascimento { get; set; }
    public string? NomeMae { get; set; }
    public string? NomePai { get; set; }
}
