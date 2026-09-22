# Migrations

As migrations anteriores foram geradas para o provider Npgsql (PostgreSQL) e são
incompatíveis com o provider `Microsoft.EntityFrameworkCore.SqlServer` (Azure SQL) que o
projeto passou a usar. Não é seguro adaptá-las manualmente — os tipos de coluna, o
`GETUTCDATE()`/`sysdatetime()` para valores padrão, as constraints e as convenções de
nome mudam entre os dois providers, e uma migration escrita errada pode corromper o
schema num banco real.

Gere a migration inicial localmente (com `dotnet` e o pacote já restaurado):

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet ef migrations add InicialSqlServer \
  --project src/Infra/GeradorCertificado.Infra.csproj \
  --startup-project src/Api/GeradorCertificado.Api.csproj
```

Isso recria esta pasta com os arquivos `*.cs`, `*.Designer.cs` e o
`GeradorCertificadoDbContextModelSnapshot.cs`. Depois é só aplicar normalmente:

```bash
dotnet ef database update \
  --project src/Infra/GeradorCertificado.Infra.csproj \
  --startup-project src/Api/GeradorCertificado.Api.csproj \
  --connection "sua-connection-string-do-azure-sql-ou-local"
```

Depois que a migration for gerada e commitada, pode apagar este arquivo.
