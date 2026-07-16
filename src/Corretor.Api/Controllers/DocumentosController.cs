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

        if (!DocumentoRequestValido(request.Papel, request.Cpf, request.CpfDependente, request.Cnpj, request.TipoParentesco))
        {
            return BadRequest();
        }

        if (!await AplicarDadosPessoaDocumento(leadId, request, cancellationToken))
        {
            return BadRequest();
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

        if (!DocumentoRequestValido(request.Papel, request.Cpf, request.CpfDependente, request.Cnpj, request.TipoParentesco))
        {
            return BadRequest();
        }

        if (!await AplicarDadosPessoaDocumento(documento.LeadId, request, cancellationToken))
        {
            return BadRequest();
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

    private static bool DocumentoRequestValido(string papel, string? cpf, string? cpfDependente, string? cnpj, string? tipoParentesco)
    {
        if (string.IsNullOrWhiteSpace(papel))
        {
            return false;
        }

        if (papel.Equals("Dependente", StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrWhiteSpace(cpf) &&
                   !string.IsNullOrWhiteSpace(tipoParentesco);
        }

        if (papel.Equals("Empresa", StringComparison.OrdinalIgnoreCase))
        {
            return !string.IsNullOrWhiteSpace(cnpj);
        }

        return true;
    }

    private async Task<bool> AplicarDadosPessoaDocumento(Guid leadId, DocumentoPessoaRequest request, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LeadId == leadId, cancellationToken);

        if (request.Papel.Equals("Titular", StringComparison.OrdinalIgnoreCase))
        {
            if (cliente?.PessoaFisicaId is null)
            {
                return true;
            }

            var titular = await db.PessoasFisicas.FirstOrDefaultAsync(x => x.Id == cliente.PessoaFisicaId.Value, cancellationToken);
            if (titular is null)
            {
                return true;
            }

            titular.Nome = request.Nome ?? titular.Nome;
            titular.Cpf = string.IsNullOrWhiteSpace(request.Cpf) ? titular.Cpf : ApenasDigitos(request.Cpf);
            titular.DataNascimento = request.DataNascimento ?? titular.DataNascimento;
            titular.NomeMae = request.NomeMae ?? titular.NomeMae;
            titular.NomePai = request.NomePai ?? titular.NomePai;
            return true;
        }

        if (!request.Papel.Equals("Dependente", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (cliente?.PessoaFisicaId is null)
        {
            return false;
        }

        var cpfDependente = string.IsNullOrWhiteSpace(request.CpfDependente) ? null : ApenasDigitos(request.CpfDependente);
        Dependente? dependente = null;

        if (!string.IsNullOrWhiteSpace(cpfDependente))
        {
            dependente = await db.Dependentes
                .FirstOrDefaultAsync(x => x.PessoaFisicaId == cliente.PessoaFisicaId.Value && x.Cpf == cpfDependente, cancellationToken);
        }

        PessoaFisica? pessoaDependente = null;
        if (dependente?.PessoaFisicaDependenteId is not null)
        {
            pessoaDependente = await db.PessoasFisicas
                .FirstOrDefaultAsync(x => x.Id == dependente.PessoaFisicaDependenteId.Value, cancellationToken);
        }

        if (pessoaDependente is null && !string.IsNullOrWhiteSpace(cpfDependente))
        {
            pessoaDependente = await db.PessoasFisicas
                .FirstOrDefaultAsync(x => x.Cpf == cpfDependente, cancellationToken);
        }

        if (pessoaDependente is null)
        {
            pessoaDependente = new PessoaFisica
            {
                Id = Guid.NewGuid(),
                Nome = string.IsNullOrWhiteSpace(request.Nome) ? "Dependente" : request.Nome,
                Cpf = cpfDependente,
                DataNascimento = request.DataNascimento,
                NomeMae = request.NomeMae,
                NomePai = request.NomePai
            };
            db.PessoasFisicas.Add(pessoaDependente);
        }
        else
        {
            pessoaDependente.Nome = request.Nome ?? pessoaDependente.Nome;
            pessoaDependente.Cpf = cpfDependente ?? pessoaDependente.Cpf;
            pessoaDependente.DataNascimento = request.DataNascimento ?? pessoaDependente.DataNascimento;
            pessoaDependente.NomeMae = request.NomeMae ?? pessoaDependente.NomeMae;
            pessoaDependente.NomePai = request.NomePai ?? pessoaDependente.NomePai;
        }

        if (dependente is null)
        {
            dependente = new Dependente
            {
                Id = Guid.NewGuid(),
                PessoaFisicaId = cliente.PessoaFisicaId.Value,
                PessoaFisicaDependenteId = pessoaDependente.Id,
                Cpf = cpfDependente
            };
            db.Dependentes.Add(dependente);
        }
        else
        {
            dependente.PessoaFisicaDependenteId = pessoaDependente.Id;
        }

        dependente.TipoParentesco = request.TipoParentesco ?? dependente.TipoParentesco;
        dependente.DataNascimento = request.DataNascimento ?? dependente.DataNascimento;
        dependente.NomeMae = request.NomeMae ?? dependente.NomeMae;
        dependente.NomePai = request.NomePai ?? dependente.NomePai;

        return true;
    }

    private static string ApenasDigitos(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }
}

public interface DocumentoPessoaRequest
{
    string? Nome { get; }
    string Papel { get; }
    string? TipoParentesco { get; }
    string? Cpf { get; }
    string? CpfDependente { get; }
    string? DataNascimento { get; }
    string? NomeMae { get; }
    string? NomePai { get; }
}

public sealed record DocumentoCreateRequest(
    [Required] Guid DocumentoExternoId,
    [Required] string Tipo,
    [Required] string Papel,
    string? TipoParentesco,
    string? Cpf,
    string? CpfDependente,
    string? Cnpj,
    bool ExtracaoProcessada,
    string? Nome,
    string? DataNascimento,
    string? NomeMae,
    string? NomePai) : DocumentoPessoaRequest;

public sealed record DocumentoUpdateRequest(
    [Required] Guid DocumentoExternoId,
    [Required] string Tipo,
    [Required] string Papel,
    string? TipoParentesco,
    string? Cpf,
    string? CpfDependente,
    string? Cnpj,
    bool ExtracaoProcessada,
    [Required] string DataUpload,
    string? Nome,
    string? DataNascimento,
    string? NomeMae,
    string? NomePai) : DocumentoPessoaRequest;

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
