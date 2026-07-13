namespace Corretor.Api.Entities;

public sealed class Historico
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public HistoricoTipo Tipo { get; set; }
    public string Data { get; set; } = string.Empty;
}
