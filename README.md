# Estapar Backend

Backend de gerenciamento de estacionamento (vagas, entrada/saída de veículos e receita), implementado
como teste técnico da Estapar. Detalhes de regras de negócio e decisões de arquitetura em
[`docs/PLANO.md`](docs/PLANO.md); progresso da implementação em [`Tasks.md`](Tasks.md) (Fase 1) e
[`Estapar.Garage/TASKS.md`](Estapar.Garage/TASKS.md) (Fase 2).

## Aplicações

A solution reúne duas Web APIs .NET independentes, lado a lado:

| Pasta | Projeto de entrada | Responsabilidade |
|---|---|---|
| `Estapar.ParkingManager/` | `Estapar.ParkingManager.Api` | Vagas, entrada/saída de veículos (webhook), cálculo de receita. Consome `GET /garage` do Estapar.Garage.Api. |
| `Estapar.Garage/` | `Estapar.Garage.Api` | Fonte de verdade da configuração de garagens: expõe `GET /garage` e um CRUD do agregado Garage (com Sectors e Spots). |

## Stack

- .NET 8 (ASP.NET Core WebAPI / Minimal API)
- Entity Framework Core + SQL Server
- Swagger / OpenAPI (Swashbuckle)
- xUnit + FluentAssertions (testes de domínio)
- Arquitetura Limpa + DDD (`Estapar.ParkingManager`: `Domain` → `Application` → `Infrastructure`/`Api`; `Estapar.Garage.Api`: mesma separação em pastas dentro de um único projeto, por ser Minimal API)

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server acessível (LocalDB ou instância local/container)
- Ferramenta `dotnet-ef` (`dotnet tool restore` se necessário)

## Configuração

### Estapar.Garage.Api

`Estapar.Garage/Estapar.Garage.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...;Database=EstaparGarageDb;..."
  }
}
```

Banco próprio (`EstaparGarageDb`), independente do `EstaparParkingManagerDb` usado pelo ParkingManager.

### Estapar.ParkingManager.Api

`Estapar.ParkingManager/Estapar.ParkingManager.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...;Database=EstaparParkingManagerDb;..."
  },
  "GarageApi": {
    "BaseUrl": "http://localhost:5010"
  }
}
```

`GarageApi:BaseUrl` deve apontar para onde o `Estapar.Garage.Api` está rodando (ou via variável de
ambiente `GarageApi__BaseUrl`).

## Como rodar

1. **Restaurar dependências**
   ```
   dotnet restore
   ```

2. **Aplicar as migrations** (cria os bancos e as tabelas):
   ```
   dotnet ef database update --project Estapar.Garage/Estapar.Garage.Api
   dotnet ef database update --project Estapar.ParkingManager/Estapar.ParkingManager.Infrastructure.Data --startup-project Estapar.ParkingManager/Estapar.ParkingManager.Api
   ```

3. **Subir o Estapar.Garage.Api primeiro** (é dele que o ParkingManager busca a configuração no boot):
   ```
   dotnet run --project Estapar.Garage/Estapar.Garage.Api
   ```
   Swagger: `http://localhost:5010/swagger`. Cadastre ao menos uma garagem via `POST /garages` antes
   de subir o ParkingManager, ou ele fará o bootstrap com uma configuração vazia.

4. **Rodar o Estapar.ParkingManager.Api**:
   ```
   dotnet run --project Estapar.ParkingManager/Estapar.ParkingManager.Api
   ```
   Na inicialização, a API busca `GET /garage` no `Estapar.Garage.Api` e popula `Sectors`/`Spots` no
   seu próprio banco (idempotente — pode reiniciar sem duplicar dados). Se o Garage.Api não estiver
   acessível, a aplicação falha ao subir (dependência obrigatória, conforme o requisito "ao iniciar a
   solução, busque e armazene os dados da garagem").

   Swagger: `http://localhost:5003/swagger`.

## Endpoints

### Estapar.Garage.Api (`http://localhost:5010`)

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/garage` | Todas as garagens, no mesmo formato agregado (com setores e vagas) de `GET /garages/{id}` — contrato consumido pelo ParkingManager. |
| `GET` | `/garages/{id}` | Busca uma garagem (com setores e vagas) por Id. |
| `POST` | `/garages` | Cria uma garagem com seus setores e vagas. |
| `PUT` | `/garages/{id}` | Atualiza nome/setores/vagas de uma garagem (substitui o conjunto de setores/vagas). |
| `DELETE` | `/garages/{id}` | Remove (soft delete) uma garagem e seus setores/vagas. |

### Estapar.ParkingManager.Api (`http://localhost:5003`)

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/webhook` | Recebe eventos `ENTRY`, `PARKED`, `EXIT` do simulador. Retorna `200`. |
| `GET` | `/revenue` | Receita total por setor (Id) e data. Aceita `{ "date": "2025-01-01", "sector": 1 }` no corpo ou `?date=2025-01-01&sector=1` na query string. `sector` é o Id do setor, não o nome — o mesmo nome (ex. "A") pode repetir entre garagens diferentes, então a consulta precisa ser por uma instância específica. |

## Testes

```
dotnet test Estapar.ParkingManager/Estapar.ParkingManager.Domain.Tests
```

Cobre `PricingPolicy` (limiares de ocupação 25/50/75/100%) e `FeeCalculator` (30 min grátis, arredondamento de hora, aplicação do fator de preço).

## Suposições assumidas fora da spec

- O payload de `ENTRY` do `.docx` não inclui `sector`, mas o fator de preço dinâmico precisa ser travado no momento da entrada (por setor). Assumimos que o simulador envia um campo adicional `"sector"` no evento `ENTRY`. Caso o simulador real não envie esse campo, o `HandleWebhookEventUseCase` rejeita o evento com `400 Bad Request` — nesse cenário, a resolução do setor precisaria migrar para o evento `PARKED` (ver nota em `docs/PLANO.md`).
- `GET /revenue` com corpo em uma requisição GET é atípico; a API aceita tanto o body quanto query string para maior compatibilidade com clientes/ferramentas que não enviam corpo em GET.
- `PUT /garages/{id}` substitui integralmente o conjunto de setores/vagas da garagem (soft-delete dos antigos + criação dos novos), em vez de reconciliar item a item por Id — mais simples e seguro para um CRUD escopado no agregado Garage.

## Estrutura do projeto

```
Estapar.ParkingManager/
  Estapar.ParkingManager.Domain/              # Entidades, VOs, regras de negócio puras
  Estapar.ParkingManager.Application/         # Casos de uso, DTOs, interfaces (ports)
  Estapar.ParkingManager.Infrastructure.Data/ # EF Core, repositórios, cliente HTTP do Estapar.Garage.Api
  Estapar.ParkingManager.Api/                 # Controllers, DI, Swagger, hosted service de bootstrap
  Estapar.ParkingManager.Domain.Tests/        # Testes unitários de domínio (xUnit)
Estapar.Garage/
  Estapar.Garage.Api/                         # Minimal API — Domain/Application/Infrastructure/Endpoints em pastas
    Domain/Entities/                          # Garage, Sector, Spot
    Application/                              # DTOs, interfaces, GarageService (casos de uso)
    Infrastructure/Persistence/               # GarageDbContext, configurations, migrations, repositório
    Endpoints/                                # GarageEndpoints (Minimal API routes)
```
