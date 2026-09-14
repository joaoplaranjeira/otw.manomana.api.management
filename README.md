# ManoMana API

Web API para gestão do evento, palpites (incluindo a altura do bebé), nascimento e ranking do **manomana.pt**. Implementada em .NET 10, ASP.NET Core, Entity Framework Core e MySQL, com uma Clean Architecture pragmática:

```text
Controller -> Service -> Repository -> DbContext -> MySQL
```

Os dados reais do nascimento, incluindo a altura, nunca são incluídos em respostas públicas enquanto o evento não estiver no estado `Published`.

## Configuração

A connection string e os restantes secrets são configurados através de variáveis
de ambiente:

```bash
export ConnectionStrings__Database='Server=...;Database=...;uid=...;password=...'
export Jwt__SigningKey='uma-chave-aleatoria-com-pelo-menos-32-bytes'
export AdminSeed__Username='admin'
export AdminSeed__Password='uma-password-inicial-forte'
```

As credenciais `AdminSeed` só são usadas para criar o primeiro administrador quando esse username ainda não existe. Podem ser removidas do ambiente depois do primeiro arranque. A migration inicial é aplicada automaticamente no arranque.

Para restringir a janela dos palpites em torno da data prevista do parto:

```bash
export Predictions__EstimatedDueDate='2026-10-01'
export Predictions__AllowedDaysBefore='30'
export Predictions__AllowedDaysAfter='30'
```

## Executar

```bash
dotnet run --project src/ManoMana.Api
```

Swagger fica disponível em `/swagger` e o health check, incluindo a base de dados, em `/health`.

## Docker Compose

O Compose contém apenas a API; não cria nem executa MySQL. Copiar `.env.example`
para `.env`, preencher a connection string remota e os restantes secrets, e executar:

```bash
docker compose down --remove-orphans
docker compose up --build --remove-orphans
```

A API fica em `http://localhost:8080` e liga diretamente ao servidor MySQL remoto
configurado em `CONNECTION_STRING_DATABASE` no ficheiro `.env`.

No VS Code/Codex, também pode ser iniciada através de **Run Task** com a task
`ManoMana: Start API in Docker Desktop`.

## Deploy no Heroku

O workflow `.github/workflows/deploy-heroku.yml` executa os testes e publica a
imagem Docker no Heroku quando existem alterações na branch `main`. Também pode
ser executado manualmente através de **Actions → Deploy to Heroku → Run workflow**.

Configurar no repositório GitHub:

- secret `HEROKU_API_KEY` com uma API key do Heroku;
- variable `HEROKU_APP_NAME` com o nome da aplicação Heroku.

Configurar as variáveis da aplicação diretamente no Heroku:

```bash
heroku config:set --app nome-da-app \
  ConnectionStrings__Database='Server=...;Database=...;uid=...;password=...' \
  Jwt__Issuer='ManoMana' \
  Jwt__Audience='ManoMana' \
  Jwt__SigningKey='uma-chave-aleatoria-com-pelo-menos-32-bytes' \
  AdminSeed__Username='admin' \
  AdminSeed__Password='uma-password-inicial-forte'
```

O Heroku fornece a porta através da variável `PORT`; a API configura o Kestrel
automaticamente para escutar nessa porta. As migrations são aplicadas durante o
arranque da aplicação.

## Testes

```bash
dotnet test ManoMana.sln
```

Os testes de integração usam EF Core InMemory e não precisam de uma instância MySQL. Incluem o fluxo completo e a verificação crítica de que o nascimento continua privado entre `Born` e `Published`.

## Rotas

Públicas: `GET /api/event`, criação/consulta/edição de palpites, estatísticas e ranking. Administrativas: login, listagem/exportação/remoção de palpites, abertura/fecho do evento, registo/edição/publicação do nascimento. As rotas administrativas, exceto login, exigem `Authorization: Bearer {jwt}`; a edição pública de um palpite exige `Authorization: Prediction {editToken}`.
