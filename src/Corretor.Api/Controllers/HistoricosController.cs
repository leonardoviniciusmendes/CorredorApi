using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class HistoricosController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/leads/{leadId:guid}/historicos")]
    public async Task<ActionResult<IEnumerable<HistoricoResponse>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var historicos = await db.Historicos.AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .Select(x => new HistoricoResponse(x.Id, x.LeadId, x.Tipo, x.Data))
            .ToListAsync(cancellationToken);

        return Ok(historicos);
    }

    [HttpPost("api/leads/{leadId:guid}/historicos")]
    public async Task<ActionResult<HistoricoResponse>> Post(Guid leadId, HistoricoRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Leads.AsNoTracking().AnyAsync(x => x.Id == leadId, cancellationToken))
        {
            return NotFound();
        }

        var historico = new Historico { Id = Guid.NewGuid(), LeadId = leadId, Tipo = request.Tipo, Data = request.Data };
        db.Historicos.Add(historico);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/leads/{leadId}/historicos", new HistoricoResponse(historico.Id, historico.LeadId, historico.Tipo, historico.Data));
    }

    [HttpPut("api/historicos/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, HistoricoRequest request, CancellationToken cancellationToken)
    {
        var historico = await db.Historicos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (historico is null)
        {
            return NotFound();
        }

        historico.Tipo = request.Tipo;
        historico.Data = request.Data;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/historicos/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var historico = await db.Historicos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (historico is null)
        {
            return NotFound();
        }

        db.Historicos.Remove(historico);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record HistoricoRequest(HistoricoTipo Tipo, [Required] string Data);
public sealed record HistoricoResponse(Guid Id, Guid LeadId, HistoricoTipo Tipo, string Data);
