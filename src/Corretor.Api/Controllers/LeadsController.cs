using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
[Route("api/leads")]
public sealed class LeadsController(CorretorDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeadResponse>>> Get(CancellationToken cancellationToken)
    {
        var leads = await db.Leads.AsNoTracking()
            .Select(x => new LeadResponse(x.Id, x.Nome, x.Telefone, x.QuantidadeVidas, x.Operadora, x.Email, x.DataEnvio, x.DataRetorno, x.DataAprovacao, x.WorkflowEtapa))
            .ToListAsync(cancellationToken);

        return Ok(leads);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeadResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LeadResponse(x.Id, x.Nome, x.Telefone, x.QuantidadeVidas, x.Operadora, x.Email, x.DataEnvio, x.DataRetorno, x.DataAprovacao, x.WorkflowEtapa))
            .FirstOrDefaultAsync(cancellationToken);

        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpPost]
    public async Task<ActionResult<LeadResponse>> Post(LeadRequest request, CancellationToken cancellationToken)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Telefone = request.Telefone,
            QuantidadeVidas = request.QuantidadeVidas,
            Operadora = request.Operadora,
            Email = request.Email,
            DataEnvio = request.DataEnvio,
            DataRetorno = request.DataRetorno,
            DataAprovacao = request.DataAprovacao,
            WorkflowEtapa = LeadWorkflowEtapa.Lead
        };

        db.Leads.Add(lead);
        db.Historicos.Add(new Historico
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            Tipo = HistoricoTipo.CadastroLead,
            Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        });
        await db.SaveChangesAsync(cancellationToken);

        var response = new LeadResponse(lead.Id, lead.Nome, lead.Telefone, lead.QuantidadeVidas, lead.Operadora, lead.Email, lead.DataEnvio, lead.DataRetorno, lead.DataAprovacao, lead.WorkflowEtapa);
        return CreatedAtAction(nameof(GetById), new { id = lead.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, LeadRequest request, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        lead.Nome = request.Nome;
        lead.Telefone = request.Telefone;
        lead.QuantidadeVidas = request.QuantidadeVidas;
        lead.Operadora = request.Operadora;
        lead.Email = request.Email;
        lead.DataEnvio = request.DataEnvio;
        lead.DataRetorno = request.DataRetorno;
        lead.DataAprovacao = request.DataAprovacao;

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        db.Leads.Remove(lead);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record LeadRequest([Required] string Nome, [Required] string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao);
public sealed record LeadResponse(Guid Id, string Nome, string Telefone, int QuantidadeVidas, string? Operadora, string? Email, string? DataEnvio, string? DataRetorno, string? DataAprovacao, LeadWorkflowEtapa WorkflowEtapa);
