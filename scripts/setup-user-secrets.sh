#!/usr/bin/env bash
# Configura os User Secrets necessários para rodar a API em ambiente de
# desenvolvimento. Nenhum desses valores deve ir para o appsettings.json
# nem para o appsettings.Development.json (eles ficam fora do repositório,
# em ~/.microsoft/usersecrets/<UserSecretsId>/secrets.json).
#
# Uso:
#   ./scripts/setup-user-secrets.sh
#
# Os valores de ConnectionStrings abaixo são os padrões de desenvolvimento
# local, equivalentes ao docker-compose.yml deste repositório. Ajuste
# conforme o seu ambiente, ou sobrescreva uma chave específica depois com:
#   dotnet user-secrets set "ConnectionStrings:AzureSQL" "outro-valor" --project src/Api

set -euo pipefail

PROJETO="src/Api"

echo "Configurando User Secrets em $PROJETO..."

dotnet user-secrets set "ConnectionStrings:AzureSQL" \
  "Server=localhost,1433;Database=GeradorCertificadoAppDb;User Id=sa;Password=SenhaForte#2026;TrustServerCertificate=True;" \
  --project "$PROJETO"

dotnet user-secrets set "ConnectionStrings:RabbitMq" \
  "amqp://guest:guest@localhost:5672" \
  --project "$PROJETO"

echo "Pronto. Falta só a Jwt:Key, que este script deixa de propósito de fora"
echo "(cada dev deve ter a sua). Gere uma e configure com:"
echo "  dotnet user-secrets set \"Jwt:Key\" \"\$(openssl rand -base64 48)\" --project $PROJETO"
echo
echo "Para conferir os valores salvos:"
echo "  dotnet user-secrets list --project $PROJETO"
