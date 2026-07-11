# Plano — Estapar Backend Developer Test (V1.4)

## Contexto

O teste técnico da Estapar pede um backend para **gerenciar um estacionamento**: controlar vagas, entrada/saída de veículos e calcular receita. Um **simulador externo** expõe a configuração da garagem em `GET /garage` e envia eventos (`ENTRY`, `PARKED`, `EXIT`) para o **webhook da nossa aplicação**.

Stack: **.NET 8 + ASP.NET Core WebAPI**, **Arquitetura Limpa + DDD**, **EF Core + SQL Server (LocalDB)** e **Swagger/OpenAPI**.

### Decisões
- **Testes:** apenas camada de **Domínio** (unitários de tarifa, preço dinâmico, ocupação).
- **Infra:** **SQL Server LocalDB** (instância local); o simulador roda à parte, manualmente.
- **Preço dinâmico:** o fator de ocupação é **calculado e travado na ENTRADA** e reaplicado na saída.

---

## Endpoints da nossa aplicação (a implementar)
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/webhook` | Recebe eventos `ENTRY`, `PARKED`, `EXIT` (payloads distintos, discriminados por `event_type`). Retorna `HTTP 200`. |
| `GET` | `/revenue` | Receita total por setor e data. Request `{ "date": "2025-01-01", "sector": "A" }` → Response `{ "amount": 0.00, "currency": "BRL", "timestamp": "..." }`. |

Endpoint **consumido** (do simulador, na inicialização): `GET /garage` → `{ "garage": [{sector, basePrice, max_capacity}], "spots": [{id, sector, lat, lng}] }`.

---

## Regras de negócio
1. **ENTRY:** registra veículo; ocupa uma vaga do setor. Calcula e **trava** o fator de preço dinâmico conforme a ocupação **no momento da entrada**.
2. **PARKED:** associa `lat`/`lng` a uma vaga específica (marca a vaga como ocupada por aquela placa).
3. **EXIT:** libera a vaga e calcula o valor:
   - Primeiros **30 min grátis**.
   - Após 30 min: **tarifa fixa por hora, arredondada para cima** (`Math.Ceiling` das horas), usando `basePrice` do setor **× fator travado na entrada**.
4. **Bloqueio:** com lotação **100%**, recusar novas entradas (`ENTRY`) até liberar vaga.
5. **Preço dinâmico (por ocupação do setor na entrada):**
   | Ocupação | Fator |
   |---|---|
   | < 25% | × 0.90 |
   | ≤ 50% | × 1.00 |
   | ≤ 75% | × 1.10 |
   | ≤ 100% | × 1.25 |

---

## Arquitetura (Clean Architecture + DDD)

Solução `Estapar.sln` com 4 projetos + 1 de testes:

```
src/
  Estapar.Domain/          # Entidades, VOs, regras puras (sem dependências)
  Estapar.Application/     # Casos de uso, DTOs, interfaces (ports)
  Estapar.Infrastructure/  # EF Core, repositórios, HttpClient do simulador
  Estapar.Api/             # WebAPI, controllers, DI, Swagger, hosted service
tests/
  Estapar.Domain.Tests/    # xUnit — regras de domínio
```

Regra de dependência: `Api → Application → Domain` e `Infrastructure → Application/Domain`. Domain não referencia ninguém.

### Estapar.Domain
- **Aggregates/Entidades:**
  - `Sector` (Name, BasePrice, MaxCapacity) — método `PriceFactorFor(occupancyRatio)` e `IsFull`.
  - `Spot` (Id, SectorName, Lat, Lng, Status).
  - `ParkingSession` (LicensePlate, SectorName, EntryTime, ExitTime?, LockedPriceFactor, SpotId?, AmountCharged?) — agregado do ciclo entrada→saída.
- **Value Objects:** `LicensePlate`, `Money` (Amount + Currency BRL).
- **Domain Services / lógica pura:**
  - `PricingPolicy.CalculateFactor(occupancyRatio)` → aplica a tabela de preço dinâmico.
  - `FeeCalculator.Calculate(entry, exit, basePrice, factor)` → 30 min grátis + `Ceiling` horas × basePrice × factor.
- **Exceções de domínio:** `GarageFullException`, `SessionNotFoundException`.

### Estapar.Application
- **Ports (interfaces):** `IGarageConfigClient` (busca `GET /garage`), `ISectorRepository`, `ISpotRepository`, `IParkingSessionRepository`, `IUnitOfWork`.
- **Use cases (handlers):**
  - `HandleEntryCommand` — valida lotação, cria `ParkingSession`, trava fator.
  - `HandleParkedCommand` — vincula vaga por lat/lng à sessão aberta.
  - `HandleExitCommand` — fecha sessão, calcula `Money`, libera vaga.
  - `GetRevenueQuery` — soma `AmountCharged` das sessões encerradas por setor+data.
  - `LoadGarageConfiguration` — busca do simulador e persiste setores/vagas (idempotente).
- **DTOs:** `WebhookEventDto` (deserialização polimórfica por `event_type`), `RevenueRequest/Response`.

### Estapar.Infrastructure
- `AppDbContext` (EF Core) + configurações Fluent API; tabelas: `Sectors`, `Spots`, `ParkingSessions`.
- Repositórios concretos implementando as ports.
- `GarageConfigHttpClient : IGarageConfigClient` via `HttpClientFactory` (base URL do simulador em config).
- Migrations EF Core (`InitialCreate`).

### Estapar.Api
- **Controllers:** `WebhookController` (`POST /webhook`), `RevenueController` (`GET /revenue`).
- **`GarageBootstrapHostedService` (`IHostedService`):** na inicialização chama `LoadGarageConfiguration` para buscar e armazenar dados da garagem/vagas.
- **DI:** registra DbContext (LocalDB), repositórios, HttpClient, handlers.
- **Swagger/OpenAPI:** `Swashbuckle.AspNetCore` com anotações; documentar os 3 tipos de evento do webhook.
- **Middleware** de tratamento de exceções → mapeia `GarageFullException` etc.; webhook responde `200` conforme spec.
- `appsettings.json`: connection string LocalDB + `Simulator:BaseUrl`.

---

## Detalhes de implementação sensíveis
- **Desserialização polimórfica do webhook:** ler `event_type` e mapear para o comando correto. Payloads diferentes por tipo (ENTRY tem `entry_time`; PARKED tem `lat`/`lng`; EXIT tem `exit_time`).
- **`GET /revenue` com corpo JSON:** a spec mostra GET com body. Implementar aceitando o body (`[FromBody]`) e, por robustez, também via query string. Documentar no README.
- **Ocupação por setor:** ratio = vagas ocupadas / `max_capacity` do setor no instante da ENTRY.
- **Idempotência do bootstrap:** ao recarregar config, não duplicar setores/vagas (upsert por chave natural).
- **Datas em UTC** (payloads usam `Z`); `GET /revenue` filtra por dia da `exit_time`.

---

## Verificação (end-to-end)
1. `dotnet build` da solução sem erros.
2. `dotnet test tests/Estapar.Domain.Tests` — regras de tarifa e preço dinâmico verdes. Casos-chave:
   - ≤30 min → R$ 0,00.
   - 31 min, basePrice 10, setor vazio (<25%) → `Ceiling(31/60)=1h × 10 × 0.90 = 9,00`.
   - Fatores nos limiares 25/50/75/100%.
3. Aplicar migrations no LocalDB (`dotnet ef database update`).
4. `dotnet run --project src/Estapar.Api` → confirmar no log que o bootstrap buscou `GET /garage` e populou setores/vagas.
5. Com o simulador apontando para a API: enviar sequência `ENTRY → PARKED → EXIT` para `POST /webhook`; verificar 200 e persistência da sessão com `AmountCharged`.
6. `GET /revenue` com `{ "date": "...", "sector": "A" }` → soma correta em BRL.
7. Swagger UI (`/swagger`) acessível e documentando os endpoints.
