using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class ContratosController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/leads/{leadId:guid}/contratos")]
    public async Task<ActionResult<IEnumerable<ContratoResponse>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var contratos = await db.Contratos.AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .Select(x => new ContratoResponse(x.Id, x.LeadId))
            .ToListAsync(cancellationToken);

        return Ok(contratos);
    }

    [HttpGet("api/contratos/{id:guid}")]
    public async Task<ActionResult<ContratoResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var contrato = await db.Contratos.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ContratoResponse(x.Id, x.LeadId))
            .FirstOrDefaultAsync(cancellationToken);

        return contrato is null ? NotFound() : Ok(contrato);
    }

    [HttpPost("api/leads/{leadId:guid}/contratos")]
    public async Task<ActionResult<ContratoResponse>> Post(Guid leadId, CancellationToken cancellationToken)
    {
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);
        if (lead is null)
        {
            return NotFound();
        }

        var contrato = new Contrato { Id = Guid.NewGuid(), LeadId = leadId };
        db.Contratos.Add(contrato);
        lead.WorkflowEtapa = LeadWorkflowEtapa.Contrato;
        db.Historicos.Add(new Historico
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            Tipo = HistoricoTipo.CriacaoContrato,
            Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
        });
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = contrato.Id }, new ContratoResponse(contrato.Id, contrato.LeadId));
    }

    [HttpDelete("api/contratos/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var contrato = await db.Contratos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (contrato is null)
        {
            return NotFound();
        }

        db.Contratos.Remove(contrato);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record ContratoResponse(Guid Id, Guid LeadId);
