# manomana.pt — Backend Specification

# 1. Objetivo

Criar a Web API do **manomana.pt**, responsável por gerir:

- evento;
- palpites;
- estatísticas;
- encerramento das apostas;
- nascimento;
- publicação do resultado;
- ranking;
- administração.

Tecnologia:

- .NET 8 ou superior;
- ASP.NET Core Web API;
- Entity Framework Core;
- MySQL.

Arquitetura pretendida:

**Clean Architecture pragmática**, com separação clara entre:

- Controllers
- Services
- Repositories
- Domain
- Persistence

Não usar CQRS/MediatR no MVP.

O objetivo é manter uma arquitetura simples, explícita e fácil de manter.

---

# 2. Princípios

Aplicar:

- SOLID;
- dependency inversion;
- interfaces;
- dependency injection;
- async/await;
- CancellationToken;
- DTOs;
- validação;
- logging;
- global exception handling.

Evitar:

- lógica de negócio nos controllers;
- controllers a aceder diretamente ao DbContext;
- repositories com regras de negócio;
- DTOs iguais às entities por conveniência;
- generic repository abstrato em excesso.

---

# 3. Solution

Sugestão:

```text
ManoMana.sln

src/
  ManoMana.Api/
  ManoMana.Application/
  ManoMana.Domain/
  ManoMana.Infrastructure/

tests/
  ManoMana.UnitTests/
  ManoMana.IntegrationTests/
```

---

# 4. Dependências

```text
Api
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application
 ↓
Domain
```

`Api` referencia:

```text
Application
Infrastructure
```

`Infrastructure` referencia:

```text
Application
Domain
```

`Domain` não referencia nenhum projeto.

---

# 5. Responsabilidades

## ManoMana.Api

Contém:

- Controllers;
- middleware;
- authentication configuration;
- Swagger;
- API configuration;
- dependency injection bootstrap;
- health checks.

---

## ManoMana.Application

Contém:

- Services;
- interfaces dos services;
- interfaces dos repositories;
- DTOs;
- requests;
- responses;
- validators;
- scoring engine;
- application exceptions.

---

## ManoMana.Domain

Contém:

- entities;
- enums;
- value objects simples;
- regras invariantes do domínio quando apropriado.

---

## ManoMana.Infrastructure

Contém:

- EF Core;
- DbContext;
- entity configurations;
- migrations;
- repository implementations;
- database access;
- authentication implementation;
- persistence helpers.

---

# 6. Estrutura

```text
ManoMana.Domain/
  Entities/
    Event.cs
    Prediction.cs
    Birth.cs
    AdminUser.cs

  Enums/
    Gender.cs
    EventStatus.cs

ManoMana.Application/
  Interfaces/
    Services/
      IEventService.cs
      IPredictionService.cs
      IRankingService.cs
      IAdminService.cs
      IAuthenticationService.cs

    Repositories/
      IEventRepository.cs
      IPredictionRepository.cs
      IBirthRepository.cs
      IAdminUserRepository.cs

  Services/
    EventService.cs
    PredictionService.cs
    RankingService.cs
    AdminService.cs
    AuthenticationService.cs

  DTOs/
  Requests/
  Responses/
  Validators/
  Scoring/

ManoMana.Infrastructure/
  Persistence/
    ManoManaDbContext.cs
    Configurations/
    Migrations/

  Repositories/
    EventRepository.cs
    PredictionRepository.cs
    BirthRepository.cs
    AdminUserRepository.cs

ManoMana.Api/
  Controllers/
    EventController.cs
    PredictionsController.cs
    RankingController.cs
    AdminController.cs
    AuthController.cs

  Middleware/
  Extensions/
```

---

# 7. Domain

## Gender

```csharp
public enum Gender
{
    Boy = 1,
    Girl = 2
}
```

---

## EventStatus

```csharp
public enum EventStatus
{
    Open = 1,
    Closed = 2,
    Born = 3,
    Published = 4
}
```

---

# 8. Event entity

```csharp
public class Event
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public EventStatus Status { get; set; }

    public DateTimeOffset? PredictionsCloseAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
```

Para o MVP existirá apenas um evento ativo.

Não hardcode o seu ID na aplicação.

---

# 9. Prediction entity

```csharp
public class Prediction
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string ParticipantName { get; set; } = default!;

    public Gender Gender { get; set; }

    public DateOnly? PredictedBirthDate { get; set; }

    public TimeOnly? PredictedBirthTime { get; set; }

    public int? PredictedWeightGrams { get; set; }

    public int? PredictedHeightCentimeters { get; set; }

    public string? PredictedName { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public string? EditTokenHash { get; set; }

    public Event Event { get; set; } = default!;
}
```

---

# 10. Birth entity

Os dados reais do bebé deverão ficar numa entity separada para reduzir o risco de exposição acidental antes do reveal.

```csharp
public class Birth
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Gender Gender { get; set; }

    public DateOnly BirthDate { get; set; }

    public TimeOnly BirthTime { get; set; }

    public int WeightGrams { get; set; }

    public int HeightCentimeters { get; set; }

    public string Name { get; set; } = default!;

    public string? PhotoUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public Event Event { get; set; } = default!;
}
```

---

# 11. Segurança crítica

Antes do estado `Published`:

**NENHUM endpoint público deverá devolver:**

- sexo real;
- nome real;
- data real;
- hora real;
- peso real;
- altura real;
- fotografia.

Mesmo no estado `Born`.

Não devolver Birth para o frontend e esconder via CSS.

A proteção deve existir no backend.

---

# 12. AdminUser

```csharp
public class AdminUser
{
    public Guid Id { get; set; }

    public string Username { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
}
```

Nunca guardar passwords em plaintext.

Utilizar password hashing seguro.

---

# 13. DTO público do evento

```csharp
public sealed record EventResponse(
    Guid Id,
    string Name,
    EventStatus Status,
    DateTimeOffset? PredictionsCloseAt,
    BirthPublicResponse? Birth
);
```

Regra:

```text
Status = Published
    => Birth pode ser preenchido

qualquer outro status
    => Birth = null
```

---

# 14. Criar palpite

Request:

```csharp
public sealed record CreatePredictionRequest(
    string Name,
    Gender Gender,
    DateOnly? PredictedBirthDate,
    TimeOnly? PredictedBirthTime,
    int? PredictedWeightGrams,
    int? PredictedHeightCentimeters,
    string? PredictedName
);
```

Response:

```csharp
public sealed record CreatePredictionResponse(
    Guid Id,
    string EditToken
);
```

---

# 15. Edit token

Após criação de um palpite, retornar um token aleatório.

Guardar apenas hash do token.

Frontend pode guardar:

```text
localStorage
```

Permite que o utilizador altere o palpite posteriormente sem criar conta.

Exemplo:

```text
PUT /api/predictions/{id}
Authorization: Prediction {token}
```

Após fecho do evento:

alteração deixa de ser permitida.

---

# 16. Validações

Nome:

```text
required
2–100 chars
```

Peso:

```text
1000–6000 g
```

Altura:

```text
30–70 cm
```

Nome previsto:

```text
max 100 chars
```

Data prevista:

definir janela razoável configurável.

Exemplo:

```text
-30 dias / +30 dias
```

em torno da data prevista do parto.

---

# 17. PredictionsController

Base route:

```text
/api/predictions
```

Endpoints:

```text
POST /api/predictions
GET  /api/predictions/{id}
PUT  /api/predictions/{id}
GET  /api/predictions/stats
```

Controller deverá:

1. validar HTTP input;
2. chamar service;
3. devolver response.

Não conter regras de negócio.

---

# 18. PredictionService

Responsável por:

- verificar estado do evento;
- criar palpite;
- validar possibilidade de alteração;
- gerar edit token;
- calcular estatísticas;
- garantir regras funcionais.

Exemplo:

```csharp
public interface IPredictionService
{
    Task<CreatePredictionResponse> CreateAsync(
        CreatePredictionRequest request,
        CancellationToken cancellationToken);

    Task<PredictionResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Guid id,
        string token,
        UpdatePredictionRequest request,
        CancellationToken cancellationToken);

    Task<PredictionStatsResponse> GetStatsAsync(
        CancellationToken cancellationToken);
}
```

---

# 19. PredictionRepository

Responsável exclusivamente por persistência.

```csharp
public interface IPredictionRepository
{
    Task<Prediction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task AddAsync(
        Prediction prediction,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Prediction>> GetByEventAsync(
        Guid eventId,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

Se necessário, criar queries agregadas específicas para estatísticas para evitar carregar todos os palpites.

---

# 20. Event endpoints

```text
GET /api/event
```

Retorna estado público.

Admin:

```text
POST /api/admin/event/open
POST /api/admin/event/close
```

---

# 21. Birth endpoints

Admin:

```text
POST /api/admin/birth
PUT  /api/admin/birth
POST /api/admin/birth/publish
```

Fluxo:

```text
Open
 ↓
Closed
 ↓
Born
 ↓
Published
```

---

# 22. Regras de transição

### Open → Closed

Permitido.

### Closed → Open

Permitido apenas por admin.

### Closed → Born

Permitido após guardar dados do nascimento.

### Born → Published

Permitido explicitamente.

Evitar publicação automática após registo.

### Published

Considerar irreversível no MVP.

---

# 23. Ranking

Endpoint:

```text
GET /api/ranking
```

Antes de `Published`:

```text
404
```

ou:

```text
409 Conflict
```

Preferência:

`409 Conflict`

Mensagem:

```json
{
  "code": "RESULT_NOT_PUBLISHED",
  "message": "The result has not been published yet."
}
```

---

# 24. Pontuação

Criar componente separado:

```text
IScoringService
ScoringService
```

ou:

```text
PredictionScorer
```

Preferência:

classe pura sem acesso a DB.

---

## Sexo

```text
correto = 100
errado = 0
```

---

## Data

```text
0 dias = 40
1 = 35
2 = 30
3 = 25
4 = 20
5 = 15
6 = 10
7 = 5
> 7 = 0
```

---

## Hora

Máximo:

`30`

Sugestão linear:

```text
0 minutos = 30
>= 720 minutos = 0
```

Fórmula aproximada:

```text
score = 30 * (1 - differenceMinutes / 720)
```

Clamp entre:

```text
0..30
```

---

## Peso

```text
<= 25g  = 30
<= 50g  = 25
<= 100g = 20
<= 150g = 15
<= 250g = 10
<= 400g = 5
> 400g  = 0
```

---

## Altura

```text
0 cm  = 30
1 cm  = 25
2 cm  = 20
3 cm  = 15
4 cm  = 10
5 cm  = 5
> 5 cm = 0
```

---

# 25. RankingResponse

```csharp
public sealed record RankingEntryResponse(
    int Position,
    Guid PredictionId,
    string ParticipantName,
    int TotalScore,
    bool GenderCorrect,
    int? BirthDateDifferenceDays,
    int? BirthTimeDifferenceMinutes,
    int? WeightDifferenceGrams,
    int? HeightDifferenceCentimeters,
    bool NameCorrect
);
```

---

# 26. Ordenação do ranking

1. maior TotalScore;
2. sexo correto;
3. menor diferença de data;
4. menor diferença de hora;
5. menor diferença de peso;
6. menor diferença de altura;
7. CreatedAt mais antigo.

---

# 27. Estatísticas

Endpoint:

```text
GET /api/predictions/stats
```

Response:

```csharp
public sealed record PredictionStatsResponse(
    int Total,
    int Boy,
    int Girl,
    decimal BoyPercentage,
    decimal GirlPercentage,
    DateOnly? AveragePredictedBirthDate,
    int? AveragePredictedWeightGrams,
    int? AveragePredictedHeightCentimeters
);
```

---

# 28. Admin authentication

Endpoint:

```text
POST /api/admin/login
```

Request:

```json
{
  "username": "...",
  "password": "..."
}
```

Response:

JWT.

---

# 29. JWT

Configuração:

```text
Issuer
Audience
SigningKey
Expiration
```

Utilizar:

```text
Authorization: Bearer {token}
```

Controllers administrativos:

```csharp
[Authorize]
```

---

# 30. API routes completas

## Public

```text
GET  /api/event

POST /api/predictions
GET  /api/predictions/{id}
PUT  /api/predictions/{id}

GET  /api/predictions/stats

GET  /api/ranking
```

---

## Admin

```text
POST /api/admin/login

GET    /api/admin/predictions
DELETE /api/admin/predictions/{id}

POST /api/admin/event/open
POST /api/admin/event/close

POST /api/admin/birth
PUT  /api/admin/birth

POST /api/admin/birth/publish
```

---

# 31. DbContext

```csharp
public class ManoManaDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<Birth> Births => Set<Birth>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public ManoManaDbContext(
        DbContextOptions<ManoManaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ManoManaDbContext).Assembly);
    }
}
```

---

# 32. Entity configurations

Não configurar mappings diretamente no DbContext.

Criar:

```text
EventConfiguration
PredictionConfiguration
BirthConfiguration
AdminUserConfiguration
```

Implementando:

```csharp
IEntityTypeConfiguration<T>
```

---

# 33. Database

Preferência:

MySQL.

Provider:

```text
Npgsql.EntityFrameworkCore.MySQL
```

---

# 34. Migrations

Guardar em:

```text
Infrastructure/Persistence/Migrations
```

---

# 35. Seed

Criar seed inicial com:

- evento `mano mana`;
- admin inicial.

Credenciais do admin devem vir de environment variables no primeiro arranque.

Nunca fazer commit da password.

---

# 36. Configuration

Usar Options Pattern.

Exemplo:

```text
JwtOptions
PredictionOptions
AdminSeedOptions
```

---

# 37. appsettings

```json
{
  "ConnectionStrings": {
    "Database": ""
  },
  "Jwt": {
    "Issuer": "ManoMana",
    "Audience": "ManoMana",
    "SigningKey": "",
    "ExpirationMinutes": 120
  }
}
```

Secrets através de environment variables.

---

# 38. Error handling

Criar middleware global.

Formato:

```json
{
  "code": "EVENT_CLOSED",
  "message": "Predictions are closed."
}
```

Mapear:

```text
ValidationException -> 400
Unauthorized -> 401
Forbidden -> 403
NotFound -> 404
Conflict -> 409
Unexpected -> 500
```

---

# 39. Logging

Usar:

```text
ILogger<T>
```

Opcional:

Serilog.

Nunca logar:

- passwords;
- JWT;
- prediction edit tokens;
- connection strings.

---

# 40. Swagger

Ativar Swagger.

Documentar:

- endpoints;
- responses;
- auth.

Swagger deverá permitir JWT Bearer.

---

# 41. CORS

Configurar domínio:

```text
https://manomana.pt
```

Em desenvolvimento permitir localhost configurável.

Não usar:

```text
AllowAnyOrigin
```

em produção.

---

# 42. Rate limiting

Adicionar rate limiting simples ao endpoint:

```text
POST /api/predictions
```

Exemplo:

```text
10 pedidos / minuto / IP
```

Isto é mitigação, não mecanismo de identidade.

---

# 43. Duplicate prevention

Não bloquear rigidamente por IP.

Famílias podem partilhar:

- Wi-Fi;
- dispositivos;
- redes.

Pode existir proteção suave.

Possíveis sinais:

- participant name;
- browser token;
- edit token.

Admin deverá conseguir remover duplicados.

---

# 44. Health check

Endpoint:

```text
GET /health
```

Verificar:

- API;
- database.

---

# 45. Docker

Criar:

```text
Dockerfile
docker-compose.yml
```

Compose local:

```text
api
mysql
```

Utilizar a imagem oficial do MySQL, preferencialmente MySQL 8.x.

Exemplo de variáveis de ambiente:

```text
MYSQL_DATABASE=manomana
MYSQL_USER=manomana
MYSQL_PASSWORD=<secret>
MYSQL_ROOT_PASSWORD=<secret>
```

Connection string típica:

```text
Server=mysql;Port=3306;Database=manomana;User=manomana;Password=<secret>;
```

---

# 46. Dockerfile

Multi-stage build.

Stages:

```text
restore
build
publish
runtime
```

Imagem runtime:

ASP.NET oficial.

---

# 47. Tests

## Unit

Testar:

- scoring;
- event transitions;
- criação de prediction;
- update após close;
- birth privacy;
- ranking.

---

## Integration

Testar:

```text
POST /predictions
GET /stats
admin login
close event
register birth
public event while Born
publish
GET /ranking
```

O teste mais importante:

### Birth privacy test

Após:

```text
POST /api/admin/birth
```

mas antes de:

```text
POST /api/admin/birth/publish
```

nenhum endpoint público pode revelar dados de nascimento.

---

# 48. Naming conventions

Código:

**Inglês**

Exemplos:

```text
Prediction
Birth
Gender
Event
Ranking
```

Texto devolvido para UI:

preferencialmente frontend-controlled.

Errors podem utilizar códigos estáveis em inglês.

---

# 49. DI

Criar extensions:

```csharp
services.AddApplication();
services.AddInfrastructure(configuration);
```

Exemplo:

```text
Application/
  DependencyInjection.cs

Infrastructure/
  DependencyInjection.cs
```

---

# 50. Repository pattern

Não criar:

```csharp
IGenericRepository<T>
```

por defeito.

Preferir repositories orientados ao domínio:

```text
IPredictionRepository
IEventRepository
IBirthRepository
```

Isto permite queries específicas sem abstrações artificiais.

---

# 51. Transactions

Publicação do nascimento deverá ser transacional quando alterar:

- Birth;
- EventStatus;
- PublishedAt.

---

# 52. Concurrency

Evitar dupla publicação.

Usar uma das opções:

- transaction;
- concurrency token;
- locking ao nível da base.

Para este produto, uma transaction com validação de estado é suficiente.

---

# 53. API versioning

Não necessário no MVP.

Usar:

```text
/api/...
```

e evitar adicionar `/v1` sem necessidade real.

---

# 54. OpenAPI

Frontend deverá poder gerar tipos no futuro.

Manter contracts previsíveis.

Não devolver entities EF diretamente.

---

# 55. Date/time

Guardar:

- audit fields como UTC `DateTimeOffset`;
- data do nascimento como `DateOnly`;
- hora como `TimeOnly`.

Timezone da aplicação:

`Europe/Lisbon`

A lógica funcional deverá ser explícita quanto ao timezone.

---

# 56. Export CSV

Admin:

```text
GET /api/admin/predictions/export
```

Pode ser acrescentado no MVP ou imediatamente depois.

Campos:

```text
Participant
Gender
PredictedDate
PredictedTime
PredictedWeight
PredictedName
CreatedAt
```

---

# 57. Arquitetura esperada de uma operação

Exemplo:

```text
HTTP Request
     ↓
PredictionsController
     ↓
IPredictionService
     ↓
PredictionService
     ↓
IPredictionRepository
     ↓
PredictionRepository
     ↓
ManoManaDbContext
     ↓
MySQL
```

---

# 58. Regra principal da arquitetura

### Controller

HTTP.

### Service

Negócio.

### Repository

Dados.

### DbContext

Persistência.

Não misturar responsabilidades.

---

# 59. Exemplo controller

```csharp
[ApiController]
[Route("api/predictions")]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionService _predictionService;

    public PredictionsController(
        IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    [HttpPost]
    public async Task<ActionResult<CreatePredictionResponse>> Create(
        CreatePredictionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _predictionService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(Get),
            new { id = response.Id },
            response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PredictionResponse>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _predictionService.GetAsync(
            id,
            cancellationToken);

        return response is null
            ? NotFound()
            : Ok(response);
    }
}
```

---

# 60. Exemplo service

```csharp
public class PredictionService : IPredictionService
{
    private readonly IEventRepository _eventRepository;
    private readonly IPredictionRepository _predictionRepository;

    public PredictionService(
        IEventRepository eventRepository,
        IPredictionRepository predictionRepository)
    {
        _eventRepository = eventRepository;
        _predictionRepository = predictionRepository;
    }

    public async Task<CreatePredictionResponse> CreateAsync(
        CreatePredictionRequest request,
        CancellationToken cancellationToken)
    {
        var currentEvent =
            await _eventRepository.GetCurrentAsync(cancellationToken);

        if (currentEvent is null)
            throw new NotFoundException("EVENT_NOT_FOUND");

        if (currentEvent.Status != EventStatus.Open)
            throw new ConflictException("EVENT_CLOSED");

        // validate
        // generate edit token
        // create entity
        // persist

        throw new NotImplementedException();
    }
}
```

---

# 61. Exemplo repository

```csharp
public class PredictionRepository : IPredictionRepository
{
    private readonly ManoManaDbContext _dbContext;

    public PredictionRepository(
        ManoManaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Prediction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Predictions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Prediction prediction,
        CancellationToken cancellationToken)
    {
        await _dbContext.Predictions.AddAsync(
            prediction,
            cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

---

# 62. MVP

Implementar por esta ordem:

1. solution architecture;
2. EF Core + MySQL;
3. Event;
4. Prediction;
5. statistics;
6. admin auth;
7. close/open;
8. Birth;
9. publish;
10. scoring;
11. ranking;
12. tests;
13. Docker.

---

# 63. Definition of Done

API deve estar pronta quando:

- compila sem warnings relevantes;
- migrations funcionam;
- Swagger funciona;
- predictions podem ser criadas;
- predictions não podem ser criadas após fecho;
- birth pode ser registado;
- birth permanece completamente secreto até publicação;
- reveal funciona;
- ranking é calculado;
- admin protegido com JWT;
- unit tests passam;
- integration tests passam;
- aplicação corre via Docker.

---

# 64. Instrução final ao Codex

Implementar código simples e explícito.

Não introduzir padrões arquiteturais sem necessidade.

A aplicação deverá seguir esta cadeia:

> Controller → Service → Repository → DbContext

A regra mais importante do domínio é:

> Os dados reais do bebé são confidenciais até o administrador executar explicitamente a publicação do reveal.

A arquitetura deve garantir esta regra no backend, independentemente do frontend.
