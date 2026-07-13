namespace Corretor.Api.Entities;

public sealed class Endereco
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string Logradouro { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
}
