namespace Corretor.Api.Entities;

public sealed class Documento
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public DocumentoCategoria Categoria { get; set; }
    public DocumentoIdentificacaoTipo? TipoIdentificacao { get; set; }
    public DocumentoEnderecoTipo? TipoEndereco { get; set; }
    public DocumentoDe DocumentoDe { get; set; }
    public string DataUpload { get; set; } = string.Empty;
    public string NomeArquivo { get; set; } = string.Empty;
    public string NomeArquivoArmazenado { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long TamanhoBytes { get; set; }
    public string CaminhoArquivo { get; set; } = string.Empty;
    public bool Aprovado { get; set; }
    public string? DataAprovacao { get; set; }
    public string? MotivoReprovacao { get; set; }
}
