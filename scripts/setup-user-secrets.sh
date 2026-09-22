#!/usr/bin/env bash
# Configura os User Secrets necessários para rodar a API em ambiente de
# desenvolvimento. Nenhum desses valores deve ir para o appsettings.json
# nem para o appsettings.Development.json (eles ficam fora do repositório,
# em ~/.microsoft/usersecrets/<UserSecretsId>/secrets.json).
#
# Uso:
#   ./scripts/setup-user-secrets.sh
#
# Os valores abaixo são os padrões de desenvolvimento local (equivalentes
# aos que estavam hardcoded no appsettings.Development.json antes desta
# limpeza). Ajuste conforme o seu ambiente antes de rodar, ou rode o
# script e depois sobrescreva uma chave específica com:
#   dotnet user-secrets set "Jwt:Key" "outro-valor" --project src/Api

set -euo pipefail

PROJETO="src/Api"

echo "Configurando User Secrets em $PROJETO..."

dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48 2>/dev/null || echo 'troque-esta-chave-por-uma-chave-secreta-forte-de-producao')" --project "$PROJETO"

dotnet user-secrets set "ConnectionStrings:PostgresEF" \
  "Host=localhost;Port=5432;Database=GeradorCertificadoAppDb;Username=postgres;Password=postgres" \
  --project "$PROJETO"

dotnet user-secrets set "ConnectionStrings:RabbitMq" \
  "amqp://guest:guest@localhost:5672" \
  --project "$PROJETO"

echo "Pronto. Para conferir os valores salvos:"
echo "  dotnet user-secrets list --project $PROJETO"
