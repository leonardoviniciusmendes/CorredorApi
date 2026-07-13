namespace Corretor.Api.Entities;

public sealed class Simulacao
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string? Link { get; set; }
    public bool Aprovada { get; set; }
    public string DataEnvio { get; set; } = string.Empty;
}
