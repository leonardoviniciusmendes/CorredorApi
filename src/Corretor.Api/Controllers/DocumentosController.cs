using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class DocumentosController(
    CorretorDbContext db,
    ExternalDocumentosClient externalDocumentosClient) : ControllerBase
{
    [HttpGet("api/leads/{leadId:guid}/documentos")]
    public async Task<ActionResult<IEnumerable<DocumentoResponse>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var documentos = await db.Documentos.AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .Select(x => new DocumentoResponse(
                x.Id,
                x.LeadId,
                x.DocumentoExternoId,
                x.Tipo,
                x.Papel,
                x.TipoParentesco,
                x.Cpf,
                x.CpfDependente,
                x.Cnpj,
                x.ExtracaoProcessada,
                x.Aprovado,
                x.DataUpload,
                x.DataAprovacao,
                x.MotivoReprovacao))
            .ToListAsync(cancellationToken);

        return Ok(documentos);
    }

    [HttpPost("api/leads/{leadId:guid}/documentos")]
    [Consumes("application/json")]
    public async Task<ActionResult<DocumentoResponse>> Post(Guid leadId, DocumentoCreateRequest request, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        var documento = new Documento
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            DocumentoExternoId = request.DocumentoExternoId,
            Tipo = request.Tipo,
            Papel = request.Papel,
            TipoParentesco = request.TipoParentesco,
            Cpf = request.Cpf,
            CpfDependente = request.CpfDependente,
            Cnpj = request.Cnpj,
            ExtracaoProcessada = request.ExtracaoProcessada,
            DataUpload = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
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

    [HttpPost("api/documentos/{id:guid}/extrair-identificacao")]
    public async Task<IActionResult> ReprocessarExtracao(Guid id, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        try
        {
            using var response = await externalDocumentosClient.Reprocessar(documento.DocumentoExternoId, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return ExternalContent(response, content);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }
    }

    [HttpGet("api/documentos/{id:guid}/identificacao")]
    public async Task<IActionResult> ObterIdentificacao(Guid id, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        try
        {
            using var response = await externalDocumentosClient.ObterIdentificacao(documento.DocumentoExternoId, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return ExternalContent(response, content);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway);
        }
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

        return Ok(new DocumentoAprovacaoResponse(documento.Id, documento.DocumentoExternoId, documento.Aprovado, documento.DataAprovacao));
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
    public async Task<IActionResult> Put(Guid id, DocumentoUpdateRequest request, CancellationToken cancellationToken)
    {
        var documento = await db.Documentos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (documento is null)
        {
            return NotFound();
        }

        documento.DocumentoExternoId = request.DocumentoExternoId;
        documento.Tipo = request.Tipo;
        documento.Papel = request.Papel;
        documento.TipoParentesco = request.TipoParentesco;
        documento.Cpf = request.Cpf;
        documento.CpfDependente = request.CpfDependente;
        documento.Cnpj = request.Cnpj;
        documento.ExtracaoProcessada = request.ExtracaoProcessada;
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

        return NoContent();
    }

    private static DocumentoResponse ToResponse(Documento documento)
    {
        return new DocumentoResponse(
            documento.Id,
            documento.LeadId,
            documento.DocumentoExternoId,
            documento.Tipo,
            documento.Papel,
            documento.TipoParentesco,
            documento.Cpf,
            documento.CpfDependente,
            documento.Cnpj,
            documento.ExtracaoProcessada,
            documento.Aprovado,
            documento.DataUpload,
            documento.DataAprovacao,
            documento.MotivoReprovacao);
    }

    private static ContentResult ExternalContent(HttpResponseMessage response, string content)
    {
        return new ContentResult
        {
            StatusCode = (int)response.StatusCode,
            Content = content,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json"
        };
    }
}

public sealed record DocumentoCreateRequest(
    [Required] Guid DocumentoExternoId,
    [Required] string Tipo,
    [Required] string Papel,
    string? TipoParentesco,
    string? Cpf,
    string? CpfDependente,
    string? Cnpj,
    bool ExtracaoProcessada);

public sealed record DocumentoUpdateRequest(
    [Required] Guid DocumentoExternoId,
    [Required] string Tipo,
    [Required] string Papel,
    string? TipoParentesco,
    string? Cpf,
    string? CpfDependente,
    string? Cnpj,
    bool ExtracaoProcessada,
    [Required] string DataUpload);

public sealed record DocumentoResponse(
    Guid Id,
    Guid LeadId,
    Guid DocumentoExternoId,
    string Tipo,
    string Papel,
    string? TipoParentesco,
    string? Cpf,
    string? CpfDependente,
    string? Cnpj,
    bool ExtracaoProcessada,
    bool Aprovado,
    string DataUpload,
    string? DataAprovacao,
    string? MotivoReprovacao);

public sealed record DocumentoAprovacaoResponse(Guid Id, Guid DocumentoExternoId, bool Aprovado, string? DataAprovacao);
public sealed record DocumentoReprovacaoRequest([Required] string Motivo);
