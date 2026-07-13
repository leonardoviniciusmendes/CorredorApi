namespace Corretor.Api.Entities;

public sealed class FaixaEtaria
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string Faixa { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
