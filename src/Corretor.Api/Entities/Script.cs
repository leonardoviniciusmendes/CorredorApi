namespace Corretor.Api.Entities;

public sealed class Script
{
    public Guid Id { get; set; }
    public ScriptEtapa Etapa { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
}
