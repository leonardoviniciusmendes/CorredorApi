namespace Corretor.Api.Entities;

public sealed class Cliente
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public Guid? PessoaFisicaId { get; set; }
    public Guid? PessoaJuridicaId { get; set; }
}
