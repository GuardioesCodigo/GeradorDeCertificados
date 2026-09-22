# Configura os User Secrets necessários para rodar a API em ambiente de
# desenvolvimento. Nenhum desses valores deve ir para o appsettings.json
# nem para o appsettings.Development.json.
#
# Uso:
#   .\scripts\setup-user-secrets.ps1

$ErrorActionPreference = "Stop"
$projeto = "src/Api"

Write-Host "Configurando User Secrets em $projeto..."

dotnet user-secrets set "ConnectionStrings:AzureSQL" `
  "Server=localhost,1433;Database=GeradorCertificadoAppDb;User Id=sa;Password=SenhaForte#2026;TrustServerCertificate=True;" `
  --project $projeto

dotnet user-secrets set "ConnectionStrings:RabbitMq" `
  "amqp://guest:guest@localhost:5672" `
  --project $projeto

Write-Host "Pronto. Falta só a Jwt:Key, que este script deixa de propósito de fora"
Write-Host "(cada dev deve ter a sua). Gere uma e configure com:"
Write-Host '  dotnet user-secrets set "Jwt:Key" "<sua-chave>" --project src/Api'
Write-Host ""
Write-Host "Para conferir os valores salvos:"
Write-Host "  dotnet user-secrets list --project $projeto"
