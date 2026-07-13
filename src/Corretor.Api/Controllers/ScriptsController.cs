using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
[Route("api/scripts")]
public sealed class ScriptsController(CorretorDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScriptResponse>>> Get([FromQuery] ScriptEtapa? etapa, CancellationToken cancellationToken)
    {
        var query = db.Scripts.AsNoTracking();

        if (etapa.HasValue)
        {
            query = query.Where(x => x.Etapa == etapa.Value);
        }

        var scripts = await query
            .Select(x => new ScriptResponse(x.Id, x.Etapa, x.Tipo, x.Mensagem))
            .ToListAsync(cancellationToken);

        return Ok(scripts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScriptResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var script = await db.Scripts.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ScriptResponse(x.Id, x.Etapa, x.Tipo, x.Mensagem))
            .FirstOrDefaultAsync(cancellationToken);

        return script is null ? NotFound() : Ok(script);
    }

    [HttpPost]
    public async Task<ActionResult<ScriptResponse>> Post(ScriptRequest request, CancellationToken cancellationToken)
    {
        var script = new Script { Id = Guid.NewGuid(), Etapa = request.Etapa, Tipo = request.Tipo, Mensagem = request.Mensagem };
        db.Scripts.Add(script);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = script.Id }, new ScriptResponse(script.Id, script.Etapa, script.Tipo, script.Mensagem));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, ScriptRequest request, CancellationToken cancellationToken)
    {
        var script = await db.Scripts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (script is null)
        {
            return NotFound();
        }

        script.Etapa = request.Etapa;
        script.Tipo = request.Tipo;
        script.Mensagem = request.Mensagem;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var script = await db.Scripts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (script is null)
        {
            return NotFound();
        }

        db.Scripts.Remove(script);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record ScriptRequest(ScriptEtapa Etapa, [Required] string Tipo, [Required] string Mensagem);
public sealed record ScriptResponse(Guid Id, ScriptEtapa Etapa, string Tipo, string Mensagem);
