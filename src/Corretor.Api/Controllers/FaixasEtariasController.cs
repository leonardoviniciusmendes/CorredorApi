using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class FaixasEtariasController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/leads/{leadId:guid}/faixas-etarias")]
    public async Task<ActionResult<IEnumerable<FaixaEtariaResponse>>> GetByLead(Guid leadId, CancellationToken cancellationToken)
    {
        var faixas = await db.FaixasEtarias.AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .Select(x => new FaixaEtariaResponse(x.Id, x.LeadId, x.Faixa, x.Quantidade))
            .ToListAsync(cancellationToken);

        return Ok(faixas);
    }

    [HttpPost("api/leads/{leadId:guid}/faixas-etarias")]
    public async Task<ActionResult<FaixaEtariaResponse>> Post(Guid leadId, FaixaEtariaRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Leads.AsNoTracking().AnyAsync(x => x.Id == leadId, cancellationToken))
        {
            return NotFound();
        }

        var faixa = new FaixaEtaria { Id = Guid.NewGuid(), LeadId = leadId, Faixa = request.Faixa, Quantidade = request.Quantidade };
        db.FaixasEtarias.Add(faixa);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/leads/{leadId}/faixas-etarias", new FaixaEtariaResponse(faixa.Id, faixa.LeadId, faixa.Faixa, faixa.Quantidade));
    }

    [HttpPut("api/faixas-etarias/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, FaixaEtariaRequest request, CancellationToken cancellationToken)
    {
        var faixa = await db.FaixasEtarias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (faixa is null)
        {
            return NotFound();
        }

        faixa.Faixa = request.Faixa;
        faixa.Quantidade = request.Quantidade;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/faixas-etarias/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var faixa = await db.FaixasEtarias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (faixa is null)
        {
            return NotFound();
        }

        db.FaixasEtarias.Remove(faixa);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record FaixaEtariaRequest([Required] string Faixa, int Quantidade);
public sealed record FaixaEtariaResponse(Guid Id, Guid LeadId, string Faixa, int Quantidade);
