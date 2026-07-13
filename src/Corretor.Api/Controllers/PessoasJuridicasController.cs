using Corretor.Api.Data;
using Corretor.Api.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
[Route("api/pessoas-juridicas")]
public sealed class PessoasJuridicasController(CorretorDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PessoaJuridicaResponse>>> Get(CancellationToken cancellationToken)
    {
        var pessoas = await db.PessoasJuridicas.AsNoTracking()
            .Select(x => new PessoaJuridicaResponse(x.Id, x.NomeEmpresa, x.Cnpj, x.IE, x.Email, x.Telefone, x.DataAbertura))
            .ToListAsync(cancellationToken);

        return Ok(pessoas);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PessoaJuridicaResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var pessoa = await db.PessoasJuridicas.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PessoaJuridicaResponse(x.Id, x.NomeEmpresa, x.Cnpj, x.IE, x.Email, x.Telefone, x.DataAbertura))
            .FirstOrDefaultAsync(cancellationToken);

        return pessoa is null ? NotFound() : Ok(pessoa);
    }

    [HttpPost]
    public async Task<ActionResult<PessoaJuridicaResponse>> Post(PessoaJuridicaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = new PessoaJuridica
        {
            Id = Guid.NewGuid(),
            NomeEmpresa = request.NomeEmpresa,
            Cnpj = request.Cnpj,
            IE = request.IE,
            Email = request.Email,
            Telefone = request.Telefone,
            DataAbertura = request.DataAbertura
        };

        db.PessoasJuridicas.Add(pessoa);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = pessoa.Id }, new PessoaJuridicaResponse(pessoa.Id, pessoa.NomeEmpresa, pessoa.Cnpj, pessoa.IE, pessoa.Email, pessoa.Telefone, pessoa.DataAbertura));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var pessoa = await db.PessoasJuridicas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pessoa is null)
        {
            return NotFound();
        }

        db.PessoasJuridicas.Remove(pessoa);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record PessoaJuridicaRequest([Required] string NomeEmpresa, [Required] string Cnpj, [Required] string IE, string? Email, string? Telefone, DateTime DataAbertura);
public sealed record PessoaJuridicaResponse(Guid Id, string NomeEmpresa, string Cnpj, string IE, string? Email, string? Telefone, DateTime DataAbertura);
