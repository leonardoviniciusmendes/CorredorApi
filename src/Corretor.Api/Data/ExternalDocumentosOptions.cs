namespace Corretor.Api.Data;

public sealed class ExternalDocumentosOptions
{
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "http://localhost:5001";
}
