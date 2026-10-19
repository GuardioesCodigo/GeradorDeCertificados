# Gerador de Certificados Online (Certifica+)

Aplicação full-stack para instituições de ensino: o **client** (Angular, pasta `client/`) oferece
cadastro, login e telas de cursos; a **API** (.NET, pasta `server/`) automatiza a geração de certificados de curso: recebe uma lista de alunos, gera um PDF
individual para cada um (QuestPDF) e disponibiliza um ZIP para download. O processamento é
assíncrono (MassTransit + RabbitMQ), então a API responde imediatamente com `202 Accepted` e o
cliente acompanha o progresso pelo endpoint de status.

## Estrutura do repositório

```
client/   Aplicação Angular (Tailwind CSS, Signal Forms, Lucide)
server/   API ASP.NET Core — Clean Architecture, testes e docker-compose
```

## Arquitetura (API)

Clean Architecture em 4 camadas, uma por projeto:

```
src/
  Dominio/    entidades, regras de negócio e portas (interfaces) — não depende de mais nada
  Aplicacao/  casos de uso (Commands/Queries + Handlers via MediatR), FluentResults para erros
  Infra/      EF Core (SQL Server / Azure SQL), Identity, QuestPDF, armazenamento em disco — implementa as portas do Dominio
  Api/        controllers, autenticação JWT, ProblemDetails — só orquestra, sem regra de negócio
```

- **CQRS com MediatR**: cada caso de uso é um `Command`/`Query` + `Handler`.
- **Erros como valor**: handlers retornam `FluentResults.Result<T>`; `ResultExtensions.ProblemDetails`
  mapeia o tipo do erro (`Validacao`, `NaoEncontrado`, `Conflito`, `NaoAutorizado`) para o status
  HTTP e o `ProblemDetails` correspondente.
- **Autenticação**: ASP.NET Core Identity (`AddIdentityCore`) para cadastro/senha + JWT próprio para
  o access token. Toda rota exige `Authorization: Bearer {token}` por padrão (fallback policy);
  `/auth/cadastro` e `/auth/login` são as únicas públicas (`[AllowAnonymous]`).
- **Processamento assíncrono**: `POST /cursos/{id}/certificados` publica uma mensagem
  (`GerarCertificadosMessage`) no RabbitMQ; o `GerarCertificadosConsumer` gera os PDFs, monta o
  ZIP e atualiza o status da solicitação.

## Rodando localmente

### API

#### 1. Suba SQL Server e RabbitMQ

```bash
docker compose up -d
```

#### 2. Configure os User Secrets

Nenhuma credencial fica no `appsettings.*.json` — tudo via
[User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
# Linux/macOS
./scripts/setup-user-secrets.sh

# Windows
.\scripts\setup-user-secrets.ps1
```

Isso configura `ConnectionStrings:AzureSQL` e `ConnectionStrings:RabbitMq` apontando para o
`docker-compose.yml` acima. A `Jwt:Key` fica de fora de propósito — gere a sua e configure
manualmente (cada dev deve ter uma chave própria):

```bash
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project src/Api
dotnet user-secrets list --project src/Api
```

#### 3. Gere e aplique a migration inicial

O projeto migrou de PostgreSQL para SQL Server (Azure SQL) e ainda não tem nenhuma migration
gerada para esse provider — veja `src/Infra/Compartilhado/Orm/Migrations/LEIA-ME.md`:

```bash
dotnet tool install --global dotnet-ef   # se ainda não tiver
dotnet ef migrations add InicialSqlServer --project src/Infra --startup-project src/Api
```

#### 4. Rode a API

```bash
dotnet run --project src/Api
```

Em `Development`, as migrations do EF Core rodam automaticamente no boot e o papel `Cliente` é
semeado. O Swagger fica em `https://localhost:7049/swagger`.

### Client

Requer Node.js compatível com Angular 22 (>= 22.22.3, 24.15+ ou 26+) e npm. Com a API rodando em
`https://localhost:7049` (perfil `https`):

```bash
cd client
npm ci
npm start
```

O client abre em `http://localhost:4200`. Em desenvolvimento ele chama `/api/...` e o dev server
encaminha para a API local (`client/proxy.conf.json`), então não é preciso configurar CORS. Confie
no certificado de desenvolvimento do .NET (`dotnet dev-certs https --trust`) se o navegador/proxy
reclamar do HTTPS.

Build e testes do client:

```bash
npm --prefix client run build
npm --prefix client test -- --watch=false
```

Observações sobre a integração com a API atual:

- **Login**: a API devolve `{ token, dataExpiracaoEmUtc }`; o client monta a sessão a partir disso e
  lê o id do usuário do claim do JWT.
- **Emissão de certificados**: na página de detalhes de um curso, informe os alunos (um nome por
  linha) e o client chama `POST /cursos/{id}/certificados`. Em seguida acompanha
  `GET /cursos/{id}/status` e `GET /cursos/{id}/certificados` a cada 2 segundos até a solicitação
  terminar, mostrando o progresso e o estado de cada certificado. Com o status `Concluido`, o botão
  "Baixar ZIP" usa `GET /cursos/{id}/certificados/download`. Enquanto há processamento em andamento
  a API recusa (409) uma nova solicitação para o mesmo curso, e o client desabilita o botão.
- **Listagem de cursos**: a API ainda não tem `GET /cursos`. A tela "Meus cursos" usa os ids dos
  cursos cadastrados pelo próprio usuário neste navegador e carrega cada um com
  `GET /cursos/{id}`. Quando a listagem existir na API, troque o corpo de `listar()` em
  `client/src/app/cursos/cursos.service.ts`.
- **Produção**: a URL da API fica em `client/src/environments/environment.ts`. A API precisa liberar
  a origem do client via CORS para o client publicado conseguir chamá-la.

## Testes

```bash
dotnet test
```

- **`tests/GeradorCertificado.UnitTests`** (MSTest + Moq): entidades de domínio e handlers da
  camada de Aplicação, com repositórios/serviços mockados. Não precisa de banco nem de RabbitMQ.
- **`tests/GeradorCertificado.IntegrationTests`** (MSTest + `WebApplicationFactory`): sobe a API
  inteira em memória (`TestServer`), com EF Core InMemory e o transporte in-memory do MassTransit
  (`Infra:MessageBrokerProvider=InMemory`) — sem dependência de SQL Server/RabbitMQ reais. Inclui um
  fluxo ponta a ponta (`FluxoCompletoE2ETests`) que passa por cadastro → login → criar curso →
  solicitar certificados → aguardar o processamento assíncrono real do consumer → baixar e validar
  o ZIP.

  > Nota: os testes ponta a ponta da API foram escritos como testes HTTP completos via
  > `WebApplicationFactory` (exercitam a pilha inteira: controller → handler → banco → mensageria →
  > consumer → armazenamento em disco), sem depender de navegador.

Nenhum dos dois projetos de teste toca no SQL Server/RabbitMQ reais do `docker-compose.yml`; eles
são totalmente isolados e podem rodar em qualquer máquina/CI sem nenhuma dependência externa.

## Endpoints

| Método | Rota | Autenticação | Sucesso |
|---|---|---|---|
| POST | `/auth/cadastro` | pública | `201 Created` |
| POST | `/auth/login` | pública | `200 OK` |
| POST | `/cursos` | Bearer | `201 Created` |
| GET | `/cursos/{cursoId}` | Bearer | `200 OK` |
| POST | `/cursos/{cursoId}/certificados` | Bearer | `202 Accepted` |
| GET | `/cursos/{cursoId}/status` | Bearer | `200 OK` |
| GET | `/cursos/{cursoId}/certificados` | Bearer | `200 OK` |
| GET | `/cursos/{cursoId}/certificados/download` | Bearer | `200 OK` (`application/zip`) |

Detalhes de request/response e regras de negócio estão na especificação técnica do projeto.

## Publicação (Azure App Service + Azure SQL + CloudAMQP)

O deploy é automatizado por `.github/workflows/main_gerador-de-certificados-2026.yml`, disparado a
cada push na `main`: `test` (bloqueia tudo se algum teste falhar) → `build` → `migrate` (aplica as
migrations no Azure SQL) → `deploy` (App Service).

Secrets que o workflow espera no GitHub (`Settings` → `Secrets and variables` → `Actions`):

| Secret | Uso |
|---|---|
| `AZURE_SQL_CONNECTION_STRING` | usada pelo job `migrate` para rodar `dotnet ef database update` |
| `AZURESQL_CONNECTION_STRING` | configurada como `ConnectionStrings__AzureSQL` no App Service |
| `RABBITMQ_CONNECTIONSTRING` | configurada como `ConnectionStrings__RabbitMq` |
| `JWT_KEY` | configurada como `jwt__Key` |
| `NEWRELIC_LICENSE_KEY` | configurada como `NewRelic__LicenseKey` |
| `LUCKYPENNY_LICENSE_KEY` | configurada como `LuckyPenny__Licensekey` |
| `AZUREAPPSERVICE_CLIENTID_...`, `AZUREAPPSERVICE_TENANTID_...`, `AZUREAPPSERVICE_SUBSCRIPTIONID_...` | login via OIDC (`azure/login@v2`) |

`AZURE_SQL_CONNECTION_STRING` e `AZURESQL_CONNECTION_STRING` normalmente apontam para o mesmo
banco — são dois secrets porque um é usado pela CLI do `dotnet ef` (formato ADO.NET puro) e o
outro vira uma variável de ambiente do App Service.
