using System.ComponentModel.DataAnnotations;
using Corretor.Api.Data;
using Corretor.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Corretor.Api.Controllers;

[ApiController]
public sealed class EnderecosController(CorretorDbContext db) : ControllerBase
{
    [HttpGet("api/clientes/{clienteId:guid}/enderecos")]
    public async Task<ActionResult<IEnumerable<EnderecoResponse>>> GetByCliente(Guid clienteId, CancellationToken cancellationToken)
    {
        var enderecos = await db.Enderecos.AsNoTracking()
            .Where(x => x.ClienteId == clienteId)
            .Select(x => new EnderecoResponse(x.Id, x.ClienteId, x.Logradouro, x.Estado, x.Cidade, x.Cep))
            .ToListAsync(cancellationToken);

        return Ok(enderecos);
    }

    [HttpPost("api/clientes/{clienteId:guid}/enderecos")]
    public async Task<ActionResult<EnderecoResponse>> Post(Guid clienteId, EnderecoRequest request, CancellationToken cancellationToken)
    {
        if (!await db.Clientes.AsNoTracking().AnyAsync(x => x.Id == clienteId, cancellationToken))
        {
            return NotFound();
        }

        var endereco = new Endereco { Id = Guid.NewGuid(), ClienteId = clienteId, Logradouro = request.Logradouro, Estado = request.Estado, Cidade = request.Cidade, Cep = request.Cep };
        db.Enderecos.Add(endereco);
        await db.SaveChangesAsync(cancellationToken);

        return Created($"/api/clientes/{clienteId}/enderecos", new EnderecoResponse(endereco.Id, endereco.ClienteId, endereco.Logradouro, endereco.Estado, endereco.Cidade, endereco.Cep));
    }

    [HttpPut("api/enderecos/{id:guid}")]
    public async Task<IActionResult> Put(Guid id, EnderecoRequest request, CancellationToken cancellationToken)
    {
        var endereco = await db.Enderecos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (endereco is null)
        {
            return NotFound();
        }

        endereco.Logradouro = request.Logradouro;
        endereco.Estado = request.Estado;
        endereco.Cidade = request.Cidade;
        endereco.Cep = request.Cep;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("api/enderecos/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var endereco = await db.Enderecos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (endereco is null)
        {
            return NotFound();
        }

        db.Enderecos.Remove(endereco);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record EnderecoRequest([Required] string Logradouro, [Required] string Estado, [Required] string Cidade, [Required] string Cep);
public sealed record EnderecoResponse(Guid Id, Guid ClienteId, string Logradouro, string Estado, string Cidade, string Cep);
