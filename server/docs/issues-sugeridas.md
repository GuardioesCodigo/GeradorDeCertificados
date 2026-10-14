# Issues sugeridas

Não tenho como criar Issues nem o Project Board diretamente no GitHub (sem acesso à API/credenciais
daqui). Segue uma lista pronta para colar em `Issues` -> `New issue`, já quebrada em subtarefas, que
serve tanto de changelog do que foi feito quanto de board para o que falta.

## Já feito (pode fechar direto, ou abrir e fechar para ficar no histórico)

- [x] **Módulo de Usuários e Autenticação**
  - [x] Cadastro de usuário (`POST /auth/cadastro`) com validação de email e senha
  - [x] Login com emissão de JWT (`POST /auth/login`)
  - [x] Proteção das demais rotas via `Authorization: Bearer {token}`
  - [x] Seed do papel `Cliente`
- [x] **Correção de bugs**
  - [x] Remover `DepedencyInjection.cs` duplicado
  - [x] Corrigir caminho de logs (`DeliveryApp` -> `GeradorCertificado`)
  - [x] Vincular `NewRelicOptions` à configuração
  - [x] Remover comando morto `AutenticarEstabelecimentoCommand`
- [x] **Segurança de configuração**
  - [x] Remover credenciais do `appsettings.Development.json`
  - [x] Scripts de setup de User Secrets (`Jwt:Key`, Postgres, RabbitMQ)
  - [x] `docker-compose.yml` para Postgres + RabbitMQ locais
- [x] **Testes automatizados**
  - [x] 48 testes unitários (domínio + handlers, MSTest/Moq)
  - [x] 27 testes de integração (WebApplicationFactory, incluindo fluxo E2E completo)

## Ainda em aberto (dependem de acesso ao GitHub/Azure que eu não tenho)

- [ ] **Organização do repositório**
  - [ ] Criar o Project Board e mover os cards conforme o progresso
  - [ ] Configurar branch protection na `main`
- [ ] **CI/CD**
  - [ ] Workflow do GitHub Actions rodando `dotnet build` + `dotnet test` a cada PR
  - [ ] Workflow de deploy para Azure App Service
  - [ ] Pipeline (ou passo manual documentado) para `dotnet ef database update` em produção
- [ ] **Publicação**
  - [ ] Provisionar Postgres (Azure Database for PostgreSQL)
  - [ ] Provisionar RabbitMQ (CloudAMQP)
  - [ ] Configurar `ConnectionStrings__PostgresEF`, `ConnectionStrings__RabbitMq` e `Jwt:Key` como
        Application Settings no App Service
- [ ] **Nice-to-have (fora do escopo da especificação, mas listado para referência)**
  - [ ] Rate limiting nos endpoints de auth
  - [ ] Refresh token (hoje o access token expira e não tem renovação)
  - [ ] Endpoint para reprocessar uma solicitação que terminou em `Falha`
