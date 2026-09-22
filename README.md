# Gerador de Certificados Online

API que automatiza a geração de certificados de curso: recebe uma lista de alunos, gera um PDF
individual para cada um (QuestPDF) e disponibiliza um ZIP para download. O processamento é
assíncrono (MassTransit + RabbitMQ), então a API responde imediatamente com `202 Accepted` e o
cliente acompanha o progresso pelo endpoint de status.

## Arquitetura

Clean Architecture em 4 camadas, uma por projeto:

```
src/
  Dominio/    entidades, regras de negócio e portas (interfaces) — não depende de mais nada
  Aplicacao/  casos de uso (Commands/Queries + Handlers via MediatR), FluentResults para erros
  Infra/      EF Core (Postgres), Identity, QuestPDF, armazenamento em disco — implementa as portas do Dominio
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

### 1. Suba Postgres e RabbitMQ

```bash
docker compose up -d
```

### 2. Configure os User Secrets

Nenhuma credencial fica no `appsettings.*.json` — tudo via
[User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
# Linux/macOS
./scripts/setup-user-secrets.sh

# Windows
.\scripts\setup-user-secrets.ps1
```

Isso configura `Jwt:Key` (gerada aleatoriamente), `ConnectionStrings:PostgresEF` e
`ConnectionStrings:RabbitMq` apontando para o `docker-compose.yml` acima. Para conferir/alterar um
valor específico:

```bash
dotnet user-secrets list --project src/Api
dotnet user-secrets set "Jwt:Key" "outro-valor" --project src/Api
```

### 3. Rode a API

```bash
dotnet run --project src/Api
```

Em `Development`, as migrations do EF Core rodam automaticamente no boot e o papel `Cliente` é
semeado. O Swagger fica em `https://localhost:7049/swagger`.

## Testes

```bash
dotnet test
```

- **`tests/GeradorCertificado.UnitTests`** (MSTest + Moq): entidades de domínio e handlers da
  camada de Aplicação, com repositórios/serviços mockados. Não precisa de banco nem de RabbitMQ.
- **`tests/GeradorCertificado.IntegrationTests`** (MSTest + `WebApplicationFactory`): sobe a API
  inteira em memória (`TestServer`), com EF Core InMemory e o transporte in-memory do MassTransit
  (`Infra:MessageBrokerProvider=InMemory`) — sem dependência de Postgres/RabbitMQ reais. Inclui um
  fluxo ponta a ponta (`FluxoCompletoE2ETests`) que passa por cadastro → login → criar curso →
  solicitar certificados → aguardar o processamento assíncrono real do consumer → baixar e validar
  o ZIP.

  > Nota: dado que esta API não tem interface visual, os testes ponta a ponta foram escritos como
  > testes HTTP completos via `WebApplicationFactory` (exercitam a pilha inteira: controller →
  > handler → banco → mensageria → consumer → armazenamento em disco), em vez de testes de
  > navegador com Playwright — que faz mais sentido para aplicações com UI.

Nenhum dos dois projetos de teste toca no Postgres/RabbitMQ reais do `docker-compose.yml`; eles são
totalmente isolados e podem rodar em qualquer máquina/CI sem nenhuma dependência externa.

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

## Publicação (Azure + CloudAMQP)

1. Provisione um Postgres (Azure Database for PostgreSQL) e um RabbitMQ (CloudAMQP).
2. Configure `ConnectionStrings:PostgresEF`, `ConnectionStrings:RabbitMq` e `Jwt:Key` como
   variáveis de ambiente/Application Settings no App Service (mesmas chaves dos User Secrets, com
   `__` no lugar de `:` — ex. `ConnectionStrings__PostgresEF`).
3. Rode `dotnet ef database update --project src/Infra --startup-project src/Api` (ou equivalente
   no pipeline de CI/CD) para aplicar as migrations — em produção elas não rodam automaticamente no
   boot (só em `Development`/`Testing`), assim como o seed do papel `Cliente`. Se preferir manter o
   seed automático em produção também, isso é uma mudança pequena e intencionalmente deixada de
   fora aqui: normalmente prefere-se controlar isso pelo pipeline, não pelo próprio processo web.
