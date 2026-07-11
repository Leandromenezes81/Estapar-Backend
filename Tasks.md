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
- [ ] `AppDbContext` + configs Fluent API (`Sectors`, `Spots`, `ParkingSessions`)
- [ ] Repositórios concretos + `UnitOfWork`
- [ ] `GarageConfigHttpClient : IGarageConfigClient` via `HttpClientFactory`
- [ ] Migration `InitialCreate` (LocalDB)

## 5. API (`Estapar.Api`)
- [ ] `Program.cs` — DI (DbContext LocalDB, repositórios, HttpClient, handlers), Swagger
- [ ] `WebhookController` (`POST /webhook`) — retorna `200`
- [ ] `RevenueController` (`GET /revenue`)
- [ ] `GarageBootstrapHostedService` (`IHostedService`) — busca `GET /garage` na inicialização
- [ ] Middleware de tratamento de exceções
- [ ] `appsettings.json` — connection string LocalDB + `Simulator:BaseUrl`

## 6. Testes de domínio (`Estapar.Domain.Tests`)
- [x] `FeeCalculatorTests` — ≤30 min grátis; 31 min; arredondamento pra cima; aplicação do fator
- [x] `PricingPolicyTests` — limiares 25 / 50 / 75 / 100%

## 7. Fechamento
- [ ] `README.md` — como rodar, aplicar migrations, apontar o simulador para a API
- [ ] `dotnet build` sem erros
- [ ] `dotnet test` verde
