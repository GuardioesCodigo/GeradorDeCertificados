# Configura os User Secrets necessários para rodar a API em ambiente de
# desenvolvimento. Nenhum desses valores deve ir para o appsettings.json
# nem para o appsettings.Development.json.
#
# Uso:
#   .\scripts\setup-user-secrets.ps1

$ErrorActionPreference = "Stop"
$projeto = "src/Api"

Write-Host "Configurando User Secrets em $projeto..."

$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$chaveJwt = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:Key" "$OXIESvZA6fGd7way8Vz5B4FpbchPTrCL" --project "src/Api"

dotnet user-secrets set "ConnectionStrings:PostgresEF" `
  "Host=localhost;Port=5432;Database=GeradorCertificadoAppDb;Username=postgres;Password=postgres" `
  --project dotnet user-secrets set "Jwt:Key" "$OXIESvZA6fGd7way8Vz5B4FpbchPTrCL" --project "src/Api"

dotnet user-secrets set "ConnectionStrings:PostgresEF" `
  "Host=localhost;Port=5432;Database=GeradorCertificadoAppDb;Username=postgres;Password=postgres" `
  --project "src/Api"

dotnet user-secrets set "ConnectionStrings:RabbitMq" `
  "amqp://guest:guest@localhost:5672" `
  --project "src/Api"

dotnet user-secrets set "ConnectionStrings:RabbitMq" `
  "amqp://guest:guest@localhost:5672" `
  --project "src/Api"

Write-Host "Pronto. Para conferir os valores salvos:"
Write-Host "  dotnet user-secrets list --project "src/Api"
