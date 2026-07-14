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

A API de documentos (`http://localhost:5001`) fica responsavel por arquivo, extracao e armazenamento do documento bruto. A Corretor.Api guarda somente o vinculo com o lead e os metadados de negocio.

## Ficha associativa

Para gerar a ficha associativa em PDF com os dados do lead:

```text
GET /api/leads/{leadId}/ficha-associativa/pdf
```

A resposta e um arquivo `application/pdf`. O front deve buscar como Blob e enviar esse arquivo no fluxo de upload da API de documentos.

O arquivo `Templates/FichaAssociativaAnaspl.pdf` e usado como base do PDF gerado; a API escreve os dados do lead/cliente por cima do modelo.

Fluxo:

1. O front envia o arquivo para `POST http://localhost:5001/api/Documentos`.
2. A API 5001 retorna o `id` do documento externo.
3. O front envia esse `id` para a Corretor.Api em `POST /api/leads/{leadId}/documentos`.
4. A Corretor.Api usa `DocumentoExternoId` para consultar ou reprocessar na API 5001.

Para salvar o vinculo no cadastro interno da Corretor.Api, use JSON:

```text
POST /api/leads/{leadId}/documentos
Content-Type: application/json
```

Payload:

```json
{
  "documentoExternoId": "3589c6de-c0d4-4fba-ba0c-f6684fedd13f",
  "tipo": "CNH",
  "papel": "Titular",
  "tipoParentesco": "Titular",
  "cpf": "12345678901",
  "cpfDependente": null,
  "cnpj": null,
  "extracaoProcessada": true
}
```

Campos persistidos pela Corretor.Api:

- `leadId`
- `documentoExternoId`
- `tipo`
- `papel`
- `tipoParentesco`
- `cpf`
- `cpfDependente`
- `cnpj`
- `extracaoProcessada`
- `aprovado`
- `dataUpload`
- `dataAprovacao`
- `motivoReprovacao`

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

Para reprocessar pela Corretor.Api, usando o documento local salvo:

```text
POST /api/documentos/{id}/extrair-identificacao
```

Para recuperar dados processados de identificacao e endereco pela Corretor.Api:

```text
GET /api/documentos/{id}/identificacao
```

Nessas duas rotas, `{id}` e o identificador local da tabela `Documento`; a Corretor.Api usa o `documentoExternoId` salvo para chamar a API 5001.

Quando o front receber dados de identificacao extraidos, pode gravar em pessoa fisica ou dependente:

- `dataNascimento`
- `nomeMae`
- `nomePai`
