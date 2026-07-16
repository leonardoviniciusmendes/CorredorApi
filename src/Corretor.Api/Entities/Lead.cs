namespace Corretor.Api.Entities;

public sealed class Lead
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public int QuantidadeVidas { get; set; }
    public string? Operadora { get; set; }
    public string? Email { get; set; }
    public string? DataEnvio { get; set; }
    public string? DataRetorno { get; set; }
    public string? DataAprovacao { get; set; }
    public string? TokenConsultaAnalise { get; set; }
    public string? RetornoAnalise { get; set; }
    public string? DataHoraEnvioAnalise { get; set; }
    public LeadWorkflowEtapa WorkflowEtapa { get; set; }
}
