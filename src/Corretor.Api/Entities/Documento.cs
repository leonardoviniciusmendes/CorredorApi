namespace Corretor.Api.Entities;

public sealed class Documento
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public Guid DocumentoExternoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Papel { get; set; } = string.Empty;
    public string? TipoParentesco { get; set; }
    public string? Cpf { get; set; }
    public string? CpfDependente { get; set; }
    public string? Cnpj { get; set; }
    public bool ExtracaoProcessada { get; set; }
    public string DataUpload { get; set; } = string.Empty;
    public bool Aprovado { get; set; }
    public string? DataAprovacao { get; set; }
    public string? MotivoReprovacao { get; set; }
}
