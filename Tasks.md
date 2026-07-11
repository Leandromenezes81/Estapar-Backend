# Tasks — Estapar Backend

Checklist de implementação. Detalhes de arquitetura e regras em [`docs/PLANO.md`](docs/PLANO.md).

## 1. Setup da solução
- [x] `git init` + `.gitignore` (.NET)
- [x] Criar `Estapar.sln` e os 4 projetos (`Domain`, `Application`, `Infrastructure`, `Api`) + `Estapar.Domain.Tests`
- [x] Configurar referências entre projetos (regra de dependência da Clean Architecture)
- [x] Adicionar pacotes NuGet:
  - `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`
  - `Swashbuckle.AspNetCore`
  - `xunit`, `xunit.runner.visualstudio`, `FluentAssertions` (testes)

## 2. Domínio (`Estapar.Domain`)
- [x] VOs `LicensePlate`, `Money` (Amount + Currency `BRL`)
- [x] Entidade `Sector` (Name, BasePrice, MaxCapacity) com `IsFull` e `PriceFactorFor(occupancyRatio)`
- [x] Entidade `Spot` (Id, SectorName, Lat, Lng, Status)
- [x] Agregado `ParkingSession` (LicensePlate, SectorName, EntryTime, ExitTime?, LockedPriceFactor, SpotId?, AmountCharged?)
- [x] `PricingPolicy.CalculateFactor(occupancyRatio)` — tabela `<25 → 0.90`, `≤50 → 1.00`, `≤75 → 1.10`, `≤100 → 1.25`
- [x] `FeeCalculator.Calculate(entry, exit, basePrice, factor)` — 30 min grátis + `Ceiling` horas × basePrice × fator
- [x] Exceções de domínio: `GarageFullException`, `SessionNotFoundException`

## 3. Application (`Estapar.Application`)
- [x] Ports: `IGarageConfigClient`, `ISectorRepository`, `ISpotRepository`, `IParkingSessionRepository`, `IUnitOfWork`
- [x] DTOs: `WebhookEventDto` (polimórfico por `event_type`), `GarageConfigDto`, `RevenueRequest`, `RevenueResponse`
- [x] `HandleEntryUseCase` — valida lotação do setor, trava fator de preço, cria sessão
- [x] `HandleParkedUseCase` — resolve vaga por lat/lng e vincula à sessão aberta
- [x] `HandleExitUseCase` — fecha sessão, calcula `Money`, libera vaga
- [x] `HandleWebhookEventUseCase` — dispatcher que lê `event_type` e roteia para o use case correto
- [x] `GetRevenueUseCase` — soma `AmountCharged` por setor + data
- [x] `LoadGarageConfigurationUseCase` — busca do simulador e persiste (idempotente)

## 4. Infrastructure (`Estapar.Infrastructure`)
- [x] `AppDbContext` + configs Fluent API (`Sectors`, `Spots`, `ParkingSessions`)
- [x] Repositórios concretos (`SectorRepository`, `SpotRepository`, `ParkingSessionRepository`) + `UnitOfWork`
- [x] `GarageConfigHttpClient : IGarageConfigClient` via `HttpClientFactory`
- [x] Migration `InitialCreate` (aplicada no LocalDB — `EstaparDb` criado com sucesso)

## 5. API (`Estapar.Api`)
- [x] `Program.cs` — DI (DbContext LocalDB, repositórios, HttpClient, use cases), Swagger
- [x] `WebhookController` (`POST /webhook`) — retorna `200`
- [x] `RevenueController` (`GET /revenue`) — aceita JSON body e fallback via query string
- [x] `GarageBootstrapHostedService` (`IHostedService`) — busca `GET /garage` na inicialização
- [x] `ExceptionHandlingMiddleware` — mapeia `GarageFullException`→409, `SessionNotFoundException`→404, `ArgumentException`/`InvalidOperationException`→400
- [x] `appsettings.json` — connection string LocalDB + `Simulator:BaseUrl`
- [x] Testado ponta a ponta com simulador mock local: bootstrap, ENTRY→PARKED→EXIT, `GET /revenue` (R$18,00 para 65min/setor vazio), bloqueio de lotação (409) e sessão inexistente (404)

## 6. Testes de domínio (`Estapar.Domain.Tests`)
- [x] `FeeCalculatorTests` — ≤30 min grátis; 31 min; arredondamento pra cima; aplicação do fator
- [x] `PricingPolicyTests` — limiares 25 / 50 / 75 / 100%

## 7. Fechamento
- [x] `README.md` — como rodar, aplicar migrations, apontar o simulador para a API
- [x] `dotnet build` sem erros
- [x] `dotnet test` verde
