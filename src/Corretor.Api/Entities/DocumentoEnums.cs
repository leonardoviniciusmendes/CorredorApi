namespace Corretor.Api.Entities;

public enum DocumentoCategoria
{
    Identificacao = 1,
    Endereco = 2
}

public enum DocumentoIdentificacaoTipo
{
    Cnpj = 1,
    Cnh = 2,
    Certidao = 3
}

public enum DocumentoEnderecoTipo
{
    ContaDeLuz = 1,
    ContaDeAgua = 2,
    ContaDeTelefone = 3
}

public enum DocumentoDe
{
    Titular = 1,
    Dependente = 2,
    Empresa = 3
}
