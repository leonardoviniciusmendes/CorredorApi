using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
[Route("api/pessoas-fisicas")]
public sealed class PessoasFisicasController(CorretorDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PessoaFisicaResponse>>> Get(CancellationToken cancellationToken)
    {
        var pessoas = await db.PessoasFisicas.AsNoTracking()
            .Select(x => new PessoaFisicaResponse(x.Id, x.Nome, x.Cpf, x.Email, x.Telefone, x.FaixaEtaria, x.DataNascimento, x.NomeMae, x.NomePai))
            .ToListAsync(cancellationToken);

        return Ok(pessoas);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PessoaFisicaResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var pessoa = await db.PessoasFisicas.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PessoaFisicaResponse(x.Id, x.Nome, x.Cpf, x.Email, x.Telefone, x.FaixaEtaria, x.DataNascimento, x.NomeMae, x.NomePai))
            .FirstOrDefaultAsync(cancellationToken);

        return pessoa is null ? NotFound() : Ok(pessoa);
    }

    [HttpPost]
    public async Task<ActionResult<PessoaFisicaResponse>> Post(PessoaFisicaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = new PessoaFisica
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Telefone = request.Telefone,
            FaixaEtaria = request.FaixaEtaria,
            DataNascimento = request.DataNascimento,
            NomeMae = request.NomeMae,
            NomePai = request.NomePai
        };

        db.PessoasFisicas.Add(pessoa);
        await db.SaveChangesAsync(cancellationToken);

        var response = new PessoaFisicaResponse(pessoa.Id, pessoa.Nome, pessoa.Cpf, pessoa.Email, pessoa.Telefone, pessoa.FaixaEtaria, pessoa.DataNascimento, pessoa.NomeMae, pessoa.NomePai);
        return CreatedAtAction(nameof(GetById), new { id = pessoa.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, PessoaFisicaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await db.PessoasFisicas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pessoa is null)
        {
            return NotFound();
        }

        pessoa.Nome = request.Nome;
        pessoa.Cpf = request.Cpf;
        pessoa.Email = request.Email;
        pessoa.Telefone = request.Telefone;
        pessoa.FaixaEtaria = request.FaixaEtaria;
        pessoa.DataNascimento = request.DataNascimento;
        pessoa.NomeMae = request.NomeMae;
        pessoa.NomePai = request.NomePai;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var pessoa = await db.PessoasFisicas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (pessoa is null)
        {
            return NotFound();
        }

        db.PessoasFisicas.Remove(pessoa);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record PessoaFisicaRequest([Required] string Nome, [Required] string Cpf, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
public sealed record PessoaFisicaResponse(Guid Id, string Nome, string Cpf, string? Email, string? Telefone, string? FaixaEtaria, string? DataNascimento, string? NomeMae, string? NomePai);
