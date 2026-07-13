# Estapar Backend

Backend de gerenciamento de estacionamento (vagas, entrada/saída de veículos e receita), implementado
como teste técnico da Estapar. Detalhes de regras de negócio e decisões de arquitetura em
[`docs/PLANO.md`](docs/PLANO.md).

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

A credencial da connection string **não fica no `appsettings.json`** (o valor commitado ali é só um
placeholder, `Password=CHANGE_ME`) — cada projeto lê a connection string real via
[.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), configurado assim:

```
dotnet user-secrets init --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1,1433;Database=EstaparGarageDb;Uid=sa;Password=<sua-senha>;MultipleActiveResultSets=true;TrustServerCertificate=True" --project Estapar.Garage/Estapar.Garage.Api

dotnet user-secrets init --project Estapar.ParkingManager/Estapar.ParkingManager.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1,1433;Database=EstaparParkingManagerDb;Uid=sa;Password=<sua-senha>;MultipleActiveResultSets=true;TrustServerCertificate=True" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

Os secrets ficam fora do repositório (`%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json` no Windows),
e o `dotnet user-secrets init` já grava o `<UserSecretsId>` correspondente no `.csproj` de cada API —
o `WebApplication.CreateBuilder` os carrega automaticamente em ambiente `Development`, sobrepondo o
placeholder do `appsettings.json`. Em outros ambientes, defina a connection string via variável de
ambiente (`ConnectionStrings__DefaultConnection`) ou outro provedor de configuração.

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

## Modelo de domínio (Estapar.ParkingManager)

- **Sector**: setor de uma garagem (ex. "A"). O `Id` é o mesmo Id retornado pelo `GET /garage` do
  Estapar.Garage.Api — não é gerado internamente. Isso importa porque o **mesmo nome de setor se
  repete entre garagens diferentes** (cada garagem tem seu próprio "A", "B", "C", "D", com Ids
  distintos); toda regra de negócio (ocupação, fator de preço dinâmico, vaga, receita) é resolvida
  pelo `Id` do setor, nunca pelo nome, para não misturar dados de garagens diferentes.
- **Spot**: vaga física, pertence a um `SectorId`. Status: enum `SpotStatus` (`AVAILABLE = 0`,
  `OCCUPIED = 1`, `DEACTIVATED = 2`).
- **ParkingSession**: ciclo ENTRY → PARKED → EXIT de um veículo, identificado por `SectorId`. O fator
  de preço dinâmico é calculado e travado no momento da ENTRY, com base na ocupação daquele setor
  específico naquele instante.
- **EventType**: enum do tipo de evento do webhook (`ENTRY = 0`, `PARKED = 1`, `EXIT = 2`).

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
| `POST` | `/webhook` | Recebe eventos `ENTRY`, `PARKED`, `EXIT` do simulador. |
| `GET` | `/revenue` | Receita total por setor (Id) e data. |

Todas as respostas (sucesso e erro) dos dois endpoints acima são envelopadas em um objeto `Response`:

```json
{
  "errorMessages": [],
  "hasErrors": false,
  "message": "Evento 'ENTRY' processado com sucesso.",
  "statusCode": 200,
  "collection": null,
  "count": 0
}
```

Em caso de erro, `hasErrors` é `true` e `errorMessages` traz uma ou mais mensagens (validação de
payload, regra de negócio violada, etc.), sempre em português.

#### `POST /webhook`

Payload único, discriminado por `event_type` (campos não usados no tipo do evento ficam nulos):

```json
{
  "event_type": "ENTRY",
  "license_plate": "ABC1234",
  "entry_time": "2026-07-13T10:00:00Z",
  "sector": 1,
  "lat": null,
  "lng": null,
  "exit_time": null
}
```

- `event_type`: `"ENTRY"`, `"PARKED"` ou `"EXIT"` (aceita também o valor numérico do enum `EventType`).
- `sector`: **Id do setor** (não o nome) — obrigatório em `ENTRY` e `PARKED`, para travar o fator de
  preço e resolver a vaga na garagem correta.
- `lat`/`lng`: obrigatórios em `PARKED`, coordenadas da vaga.
- `entry_time`/`exit_time`: obrigatórios em `ENTRY`/`EXIT`, respectivamente.

#### `GET /revenue`

Recebe `sector` (Id do setor, não o nome) e `date`. O test spec mostra `GET` com corpo JSON; a API
aceita tanto o corpo quanto query string, para compatibilidade com clientes que não enviam corpo em
requisições `GET`:

```
GET /revenue?date=2026-07-13&sector=1
```

ou

```json
{ "date": "2026-07-13", "sector": 1 }
```

O `collection` da resposta traz `{ "amount": ..., "currency": "BRL", "timestamp": ... }`.

## Testes

```
dotnet test Estapar.ParkingManager/Estapar.ParkingManager.Domain.Tests
```

Cobre `PricingPolicy` (limiares de ocupação 25/50/75/100%) e `FeeCalculator` (30 min grátis, arredondamento de hora, aplicação do fator de preço).

## Suposições assumidas fora da spec

- O payload de `ENTRY` do `.docx` não inclui `sector`, mas o fator de preço dinâmico precisa ser travado no momento da entrada (por setor). Assumimos que o simulador envia um campo adicional `"sector"` nos eventos `ENTRY` e `PARKED`, contendo o **Id do setor** (não o nome), já que o mesmo nome de setor se repete entre garagens diferentes. Caso o simulador real não envie esse campo, o `HandleWebhookEventUseCase` rejeita o evento com `400 Bad Request`.
- Pelo mesmo motivo, `GET /revenue` recebe `sector` como Id, não como nome — consultar por nome seria ambíguo entre garagens diferentes que compartilham o mesmo nome de setor.
- `GET /revenue` com corpo em uma requisição GET é atípico; a API aceita tanto o body quanto query string para maior compatibilidade com clientes/ferramentas que não enviam corpo em GET.
- `PUT /garages/{id}` substitui integralmente o conjunto de setores/vagas da garagem (soft-delete dos antigos + criação dos novos), em vez de reconciliar item a item por Id — mais simples e seguro para um CRUD escopado no agregado Garage.
- Todas as mensagens de sucesso e erro retornadas pela API (e as exceções internas que as originam) estão em português. Mensagens geradas diretamente pelo parser JSON do .NET (`System.Text.Json`) em caso de payload semanticamente inválido — ex. um valor incompatível com o tipo de um campo — permanecem em inglês, por serem produzidas pelo runtime antes de qualquer código da aplicação.

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
