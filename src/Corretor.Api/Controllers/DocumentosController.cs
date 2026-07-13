using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class DocumentosController(
    CorretorDbContext db,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("api/leads/{leadId:guid}/documentos")]
    public async Task<ActionResult<IEnumerable<DocumentoResponse>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var documentos = await db.Documentos.AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .Select(x => new DocumentoResponse(x.Id, x.LeadId, x.Categoria, x.TipoIdentificacao, x.TipoEndereco, x.DocumentoDe, x.DataUpload, x.NomeArquivo, x.NomeArquivoArmazenado, x.ContentType, x.TamanhoBytes, x.CaminhoArquivo, x.Aprovado, x.DataAprovacao, x.MotivoReprovacao))
            .ToListAsync(cancellationToken);

        return Ok(documentos);
    }

    [HttpGet("api/documentos/{id:guid}/arquivo")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        var path = Path.Combine(environment.ContentRootPath, documento.CaminhoArquivo);
        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(path);
        return File(stream, documento.ContentType, documento.NomeArquivo);
    }

    [HttpPost("api/leads/{leadId:guid}/documentos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<DocumentoResponse>> Post(Guid leadId, [FromForm] DocumentoUploadRequest request, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        if (!DocumentoValido(request.Categoria, request.TipoIdentificacao, request.TipoEndereco))
        {
            return BadRequest();
        }

        if (request.Arquivo.Length == 0)
        {
            return BadRequest();
        }

        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Arquivo.FileName)}";
        var relativeDirectory = Path.Combine("Uploads", "Documentos");
        var directory = Path.Combine(environment.ContentRootPath, relativeDirectory);
        Directory.CreateDirectory(directory);

        var relativePath = Path.Combine(relativeDirectory, storedFileName);
        var fullPath = Path.Combine(environment.ContentRootPath, relativePath);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await request.Arquivo.CopyToAsync(stream, cancellationToken);
        }

        var documento = new Documento
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            Categoria = request.Categoria,
            TipoIdentificacao = request.TipoIdentificacao,
            TipoEndereco = request.TipoEndereco,
            DocumentoDe = request.DocumentoDe,
            DataUpload = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            NomeArquivo = request.Arquivo.FileName,
            NomeArquivoArmazenado = storedFileName,
            ContentType = string.IsNullOrWhiteSpace(request.Arquivo.ContentType) ? "application/octet-stream" : request.Arquivo.ContentType,
            TamanhoBytes = request.Arquivo.Length,
            CaminhoArquivo = relativePath
        };

        db.Documentos.Add(documento);
        lead.WorkflowEtapa = LeadWorkflowEtapa.Documentacao;
        db.Historicos.Add(new Historico
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            Tipo = HistoricoTipo.EnvioDocumento,
            Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        });
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/leads/{leadId}/documentos", ToResponse(documento));
    }

    [HttpPost("api/documentos/{id:guid}/aprovar")]
    public async Task<ActionResult<DocumentoAprovacaoResponse>> Aprovar(Guid id, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        documento.Aprovado = true;
        documento.DataAprovacao = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        documento.MotivoReprovacao = null;

        await db.SaveChangesAsync(cancellationToken);

        return Ok(new DocumentoAprovacaoResponse(documento.Id, documento.Aprovado, documento.DataAprovacao));
    }

    [HttpPost("api/documentos/{id:guid}/reprovar")]
    public async Task<ActionResult<DocumentoResponse>> Reprovar(Guid id, DocumentoReprovacaoRequest request, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        documento.Aprovado = false;
        documento.DataAprovacao = null;
        documento.MotivoReprovacao = request.Motivo;

        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(documento));
    }

    [HttpPut("api/documentos/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, DocumentoMetadataRequest request, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        if (!DocumentoValido(request.Categoria, request.TipoIdentificacao, request.TipoEndereco))
        {
            return BadRequest();
        }

        documento.Categoria = request.Categoria;
        documento.TipoIdentificacao = request.TipoIdentificacao;
        documento.TipoEndereco = request.TipoEndereco;
        documento.DocumentoDe = request.DocumentoDe;
        documento.DataUpload = request.DataUpload;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/documentos/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        db.Documentos.Remove(documento);
        await db.SaveChangesAsync(cancellationToken);

        var path = Path.Combine(environment.ContentRootPath, documento.CaminhoArquivo);
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }

        return NoContent();
    }

    private static bool DocumentoValido(DocumentoCategoria categoria, DocumentoIdentificacaoTipo? tipoIdentificacao, DocumentoEnderecoTipo? tipoEndereco)
    {
        return categoria switch
        {
            DocumentoCategoria.Identificacao => tipoIdentificacao.HasValue && !tipoEndereco.HasValue,
            DocumentoCategoria.Endereco => tipoEndereco.HasValue && !tipoIdentificacao.HasValue,
            _ => false
        };
    }

    private static DocumentoResponse ToResponse(Documento documento)
    {
        return new DocumentoResponse(
            documento.Id,
            documento.LeadId,
            documento.Categoria,
            documento.TipoIdentificacao,
            documento.TipoEndereco,
            documento.DocumentoDe,
            documento.DataUpload,
            documento.NomeArquivo,
            documento.NomeArquivoArmazenado,
            documento.ContentType,
            documento.TamanhoBytes,
            documento.CaminhoArquivo,
            documento.Aprovado,
            documento.DataAprovacao,
            documento.MotivoReprovacao);
    }
}

public sealed record DocumentoMetadataRequest(
    DocumentoCategoria Categoria,
    DocumentoIdentificacaoTipo? TipoIdentificacao,
    DocumentoEnderecoTipo? TipoEndereco,
    DocumentoDe DocumentoDe,
    [Required] string DataUpload);

public sealed class DocumentoUploadRequest
{
    public DocumentoCategoria Categoria { get; set; }
    public DocumentoIdentificacaoTipo? TipoIdentificacao { get; set; }
    public DocumentoEnderecoTipo? TipoEndereco { get; set; }
    public DocumentoDe DocumentoDe { get; set; }
    [Required] public IFormFile Arquivo { get; set; } = null!;
}

public sealed record DocumentoResponse(
    Guid Id,
    Guid LeadId,
    DocumentoCategoria Categoria,
    DocumentoIdentificacaoTipo? TipoIdentificacao,
    DocumentoEnderecoTipo? TipoEndereco,
    DocumentoDe DocumentoDe,
    string DataUpload,
    string NomeArquivo,
    string NomeArquivoArmazenado,
    string ContentType,
    long TamanhoBytes,
    string CaminhoArquivo,
    bool Aprovado,
    string? DataAprovacao,
    string? MotivoReprovacao);

public sealed record DocumentoAprovacaoResponse(Guid Id, bool Aprovado, string? DataAprovacao);
public sealed record DocumentoReprovacaoRequest([Required] string Motivo);
