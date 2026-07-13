# Corretor.Api

Backend ASP.NET Core Web API em .NET 9 para um sistema de corretor de planos de saude.

## Configurar o MySQL

Crie o banco de dados:

```sql
CREATE DATABASE corretor;
```

## Atualizar a connection string

Edite `src/Corretor.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=corretor;User=root;Password=ligado01;"
  }
}
```

## Executar migrations

```bash
dotnet ef database update --project src/Corretor.Api/Corretor.Api.csproj --startup-project src/Corretor.Api/Corretor.Api.csproj
```

## Iniciar a API

```bash
dotnet run --project src/Corretor.Api/Corretor.Api.csproj
```

## Acessar o Swagger

Com a API em ambiente de desenvolvimento, acesse:

```text
http://localhost:5000/swagger
```

Se o `dotnet run` informar outra porta no console, use essa porta com `/swagger`.

## Documentos e processamento externo

A Corretor.Api nao chama API externa de documentos. O front deve chamar a API externa diretamente quando precisar processar identificacao, endereco ou reprocessamento.

Para salvar o documento no cadastro interno da Corretor.Api, use `multipart/form-data` em:

```text
POST /api/leads/{leadId}/documentos
```

Campos da Corretor.Api:

- `arquivo`
- `categoria`
- `tipoIdentificacao`
- `tipoEndereco`
- `documentoDe`

Para processar o arquivo, o front deve chamar diretamente a API externa:

```text
POST http://localhost:5001/api/Documentos
```

Campos obrigatorios na API externa:

- `tipo`
- `papel`
- `arquivo`

Regras para o front na API externa:

- Enviar `cpf` quando o documento for de endereco.
- Enviar `cpf` quando o documento for de dependente.
- Enviar `cpfDependente` quando o documento for de dependente.
- O `cpf` em documento de dependente deve ser o CPF do titular.
- Em documento comum do titular, `cpf` pode ser omitido.

Exemplo de documento de dependente na API externa:

```text
POST http://localhost:5001/api/Documentos
Content-Type: multipart/form-data

cpf=12345678901
cpfDependente=98765432100
papel=Dependente
tipo=RG
tipoParentesco=Filho
arquivo=@documento.pdf
```

Exemplo de comprovante de endereco na API externa:

```text
POST http://localhost:5001/api/Documentos
Content-Type: multipart/form-data

cpf=12345678901
papel=Titular
tipo=ComprovanteResidencia
arquivo=@comprovante.pdf
```

Depois do upload externo, o front deve capturar o `id` retornado pela API externa.

Para reprocessar:

```text
POST http://localhost:5001/api/Documentos/{id}/extrair-identificacao
```

Para recuperar dados processados de identificacao e endereco:

```text
GET http://localhost:5001/api/Documentos/{id}/identificacao
```

Se ainda nao houver extracao salva na API externa, ela deve retornar `404`.
