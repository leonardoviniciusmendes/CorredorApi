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
