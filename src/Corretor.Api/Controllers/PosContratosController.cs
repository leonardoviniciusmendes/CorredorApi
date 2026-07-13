using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class PosContratosController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/contratos/{contratoId:guid}/pos-contratos")]
    public async Task<ActionResult<IEnumerable<PosContratoResponse>>> GetByContrato(Guid contratoId, CancellationToken cancellationToken)
    {
        var posContratos = await db.PosContratos.AsNoTracking()
            .Where(x => x.ContratoId == contratoId)
            .Select(x => new PosContratoResponse(x.Id, x.ContratoId))
            .ToListAsync(cancellationToken);

        return Ok(posContratos);
    }

    [HttpPost("api/contratos/{contratoId:guid}/pos-contratos")]
    public async Task<ActionResult<PosContratoResponse>> Post(Guid contratoId, CancellationToken cancellationToken)
    {
        var contrato = await db.Contratos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == contratoId, cancellationToken);
        if (contrato is null)
        {
            return NotFound();
        }

        var posContrato = new PosContrato { Id = Guid.NewGuid(), ContratoId = contratoId };
        db.PosContratos.Add(posContrato);
        var lead = await db.Leads.FirstOrDefaultAsync(x => x.Id == contrato.LeadId, cancellationToken);
        if (lead is not null)
        {
            lead.WorkflowEtapa = LeadWorkflowEtapa.PosContrato;
            db.Historicos.Add(new Historico
            {
                Id = Guid.NewGuid(),
                LeadId = lead.Id,
                Tipo = HistoricoTipo.PosContrato,
                Data = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd")
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/contratos/{contratoId}/pos-contratos", new PosContratoResponse(posContrato.Id, posContrato.ContratoId));
    }

    [HttpDelete("api/pos-contratos/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var posContrato = await db.PosContratos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (posContrato is null)
        {
            return NotFound();
        }

        db.PosContratos.Remove(posContrato);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record PosContratoResponse(Guid Id, Guid ContratoId);
