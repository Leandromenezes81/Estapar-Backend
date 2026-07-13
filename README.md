# Estapar Backend

Backend de gerenciamento de estacionamento (vagas, entrada/saída de veículos e receita), implementado
como teste técnico da Estapar.

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

Nenhum valor sensível (senha de banco, chaves de assinatura, segredos de autenticação) fica no
`appsettings.json` — os valores commitados ali são só placeholders (`CHANGE_ME`). Os valores reais
ficam em [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), um
arquivo `secrets.json` fora do repositório (`%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json` no
Windows), associado ao projeto via um `<UserSecretsId>` no `.csproj`. O
`WebApplication.CreateBuilder` carrega esse arquivo automaticamente em ambiente `Development`,
sobrepondo o placeholder do `appsettings.json` — você não precisa editar nenhum arquivo `.json`
manualmente.

Existem **4 secrets** para configurar, 2 deles duplicados (um valor independente por API) e 1
compartilhado entre as duas APIs. Rode os comandos abaixo em um terminal (PowerShell ou o Git Bash),
na raiz do repositório.

### Passo 0 — habilitar o User Secrets em cada projeto (uma vez só)

```
dotnet user-secrets init --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets init --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

### Secret 1 — `ConnectionStrings:DefaultConnection` (senha do SQL Server)

Cada API tem seu próprio banco (`EstaparGarageDb` e `EstaparParkingManagerDb`) na mesma instância de
SQL Server. Troque `<sua-senha>` pela senha real do seu SQL Server (usuário `sa`, ou outro usuário de
sua instância):

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1,1433;Database=EstaparGarageDb;Uid=sa;Password=<sua-senha>;MultipleActiveResultSets=true;TrustServerCertificate=True" --project Estapar.Garage/Estapar.Garage.Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1,1433;Database=EstaparParkingManagerDb;Uid=sa;Password=<sua-senha>;MultipleActiveResultSets=true;TrustServerCertificate=True" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

### Secret 2 — `Jwt:Key` (chave de assinatura do token JWT)

Uma chave aleatória por API — não precisa (nem deve) ser a mesma nas duas. Gere um valor aleatório e
já use o resultado no comando de `set`, sem editar nada manualmente:

**PowerShell:**
```powershell
$bytes = New-Object byte[] 48; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:Key" "$([Convert]::ToBase64String($bytes))" --project Estapar.Garage/Estapar.Garage.Api
```
```powershell
$bytes = New-Object byte[] 48; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Jwt:Key" "$([Convert]::ToBase64String($bytes))" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

**Git Bash / Linux / macOS:**
```bash
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

### Secret 3 — `Auth:ClientSecret` (senha do `POST /auth/token`)

Também uma chave aleatória independente por API — é a "senha" usada para obter o token JWT (ver
seção [Autenticação](#autenticação) abaixo).

**PowerShell:**
```powershell
$bytes = New-Object byte[] 24; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Auth:ClientSecret" "$([Convert]::ToBase64String($bytes))" --project Estapar.Garage/Estapar.Garage.Api
```
```powershell
$bytes = New-Object byte[] 24; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
dotnet user-secrets set "Auth:ClientSecret" "$([Convert]::ToBase64String($bytes))" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

**Git Bash / Linux / macOS:**
```bash
dotnet user-secrets set "Auth:ClientSecret" "$(openssl rand -base64 24)" --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets set "Auth:ClientSecret" "$(openssl rand -base64 24)" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

### Secret 4 — `ApiKey` (protege `GET /garage`)

**Atenção: este é o único secret que precisa ser o *mesmo valor* nas duas APIs** — é a chave que o
`Estapar.ParkingManager.Api` envia no header `X-Api-Key` ao chamar `GET /garage` no
`Estapar.Garage.Api`. Gere o valor **uma vez** e reaproveite nos dois comandos:

**PowerShell:**
```powershell
$bytes = New-Object byte[] 32; [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$apiKey = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "ApiKey" "$apiKey" --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets set "GarageApi:ApiKey" "$apiKey" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

**Git Bash / Linux / macOS:**
```bash
API_KEY=$(openssl rand -base64 32)
dotnet user-secrets set "ApiKey" "$API_KEY" --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets set "GarageApi:ApiKey" "$API_KEY" --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

### Conferindo o que foi configurado

```
dotnet user-secrets list --project Estapar.Garage/Estapar.Garage.Api
dotnet user-secrets list --project Estapar.ParkingManager/Estapar.ParkingManager.Api
```

Cada projeto deve listar 3 chaves: `ConnectionStrings:DefaultConnection`, `Jwt:Key` e
`Auth:ClientSecret`, mais `ApiKey` (Garage.Api) ou `GarageApi:ApiKey` (ParkingManager.Api).

> **Nunca cole esses valores em commits, issues, PRs, chats ou qualquer lugar público** — quem tiver
> acesso a eles pode gerar tokens válidos, ler a garagem sem autorização ou acessar o banco de dados.
> Se algum desses valores vazar, gere um novo (rodando o comando de novo) e reinicie as APIs.

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

## Autenticação

Cada API tem seu **próprio** esquema de autenticação — não compartilham segredo entre si:

- **JWT** (`Authorization: Bearer <token>`) protege o CRUD `/garages` (Garage.Api) e o `/revenue`
  (ParkingManager.Api). O token é obtido em `POST /auth/token` de cada API, com `client_id`/
  `client_secret` fixos por configuração (sem base de usuários — autenticação básica, não OAuth
  completo).
- **API Key** (header `X-Api-Key`) protege só o `GET /garage` do Garage.Api — é o endpoint chamado
  pelo `Estapar.ParkingManager.Api` (bootstrap + job periódico do Quartz), então usa uma chave fixa
  compartilhada entre as duas APIs em vez de um fluxo de token completo.
- `POST /webhook` (ParkingManager.Api) continua **anônimo** — é chamado por um simulador externo,
  fora do nosso controle.

Os 3 secrets envolvidos (`Jwt:Key`, `Auth:ClientSecret` e `ApiKey`/`GarageApi:ApiKey`) são configurados
via user secrets — veja o passo a passo com os comandos de geração em
[Configuração](#configuração), acima.

`Auth:ClientId` (`"garage-admin"` no Garage.Api, `"parking-manager-admin"` no ParkingManager.Api)
já vem no `appsettings.json` — só o `ClientSecret` é sensível. Exemplo de uso:

```
POST /auth/token
{ "clientId": "garage-admin", "clientSecret": "<seu-segredo>" }

→ { "accessToken": "...", "expiresIn": 3600, "tokenType": "Bearer" }
```

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

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `POST` | `/auth/token` | — | Emite um JWT a partir de `client_id`/`client_secret`. |
| `GET` | `/garage` | `X-Api-Key` | Todas as garagens, no mesmo formato agregado (com setores e vagas) de `GET /garages/{id}` — contrato consumido pelo ParkingManager. |
| `GET` | `/garages/{id}` | JWT | Busca uma garagem (com setores e vagas) por Id. |
| `POST` | `/garages` | JWT | Cria uma garagem com seus setores e vagas. |
| `PUT` | `/garages/{id}` | JWT | Atualiza nome/setores/vagas de uma garagem (substitui o conjunto de setores/vagas). |
| `DELETE` | `/garages/{id}` | JWT | Remove (soft delete) uma garagem e seus setores/vagas. |

### Estapar.ParkingManager.Api (`http://localhost:5003`)

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| `POST` | `/auth/token` | — | Emite um JWT a partir de `client_id`/`client_secret`. |
| `POST` | `/webhook` | — | Recebe eventos `ENTRY`, `PARKED`, `EXIT` do simulador (anônimo — fora do nosso controle). |
| `GET` | `/revenue` | JWT | Receita total por setor (Id) e data. |

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
dotnet test Estapar.ParkingManager/Estapar.ParkingManager.Tests
dotnet test Estapar.Garage/Estapar.Garage.Tests
```

`Estapar.ParkingManager.Tests` organiza os testes por camada:
- `Domain/`: `PricingPolicy` (limiares de ocupação 25/50/75/100%) e `FeeCalculator` (30 min grátis, arredondamento de hora, aplicação do fator de preço).
- `Application/`: casos de uso do webhook (`HandleEntryUseCase`, `HandleExitUseCase`, `HandleParkedUseCase`, `HandleWebhookEventUseCase`) e o bootstrap da garagem (`LoadGarageConfigurationUseCase`), com fakes escritos à mão para os repositórios (sem biblioteca de mock).

`Estapar.Garage.Tests` segue a mesma convenção (`Domain/`, `Application/`), cobrindo as entidades `Garage`/`Sector`/`Spot` e o `GarageService`.

## Suposições assumidas fora da spec

- O payload de `ENTRY` do `.docx` não inclui `sector`, mas o fator de preço dinâmico precisa ser travado no momento da entrada (por setor). Assumimos que o simulador envia um campo adicional `"sector"` nos eventos `ENTRY` e `PARKED`, contendo o **Id do setor** (não o nome), já que o mesmo nome de setor se repete entre garagens diferentes. Caso o simulador real não envie esse campo, o `HandleWebhookEventUseCase` rejeita o evento com `400 Bad Request`.
- Pelo mesmo motivo, `GET /revenue` recebe `sector` como Id, não como nome — consultar por nome seria ambíguo entre garagens diferentes que compartilham o mesmo nome de setor.
- `GET /revenue` com corpo em uma requisição GET é atípico; a API aceita tanto o body quanto query string para maior compatibilidade com clientes/ferramentas que não enviam corpo em GET.
- `PUT /garages/{id}` substitui integralmente o conjunto de setores/vagas da garagem (soft-delete dos antigos + criação dos novos), em vez de reconciliar item a item por Id — mais simples e seguro para um CRUD escopado no agregado Garage.
- Todas as mensagens de sucesso e erro retornadas pela API (e as exceções internas que as originam) estão em português. Mensagens geradas diretamente pelo parser JSON do .NET (`System.Text.Json`) em caso de payload semanticamente inválido — ex. um valor incompatível com o tipo de um campo — permanecem em inglês, por serem produzidas pelo runtime antes de qualquer código da aplicação.
- Autenticação é "básica" por design: `client_id`/`client_secret` fixos por configuração (sem base de usuários, sem refresh token) e uma API Key estática só para a chamada serviço-a-serviço `GET /garage`. Cada API assina/valida seus próprios JWTs com uma chave independente — não há segredo simétrico compartilhado entre as duas.

## Estrutura do projeto

```
Estapar.ParkingManager/
  Estapar.ParkingManager.Domain/                    # Entidades, VOs, regras de negócio puras
  Estapar.ParkingManager.Application/               # Casos de uso, DTOs, interfaces (ports)
  Estapar.ParkingManager.Infrastructure.Data/        # EF Core, repositórios, cliente HTTP do Estapar.Garage.Api
  Estapar.ParkingManager.Infrastructure.BackgroundServices/ # Jobs Quartz.NET (bootstrap/re-sync da garagem)
  Estapar.ParkingManager.Api/                       # Controllers, Auth/ (JWT), Registration/*Registration (DI), Swagger
  Estapar.ParkingManager.Tests/                     # Testes unitários (xUnit), organizados por camada: Domain/, Application/
Estapar.Garage/
  Estapar.Garage.Api/                         # Minimal API — Domain/Application/Infrastructure/Endpoints em pastas
    Domain/Entities/                          # Garage, Sector, Spot
    Application/                              # DTOs, interfaces, GarageService (casos de uso)
    Infrastructure/Persistence/               # GarageDbContext, configurations, migrations, repositório
    Auth/                                     # JwtTokenService (emissão de token)
    Filters/                                  # ApiKeyEndpointFilter (protege GET /garage)
    Endpoints/                                # GarageEndpoints, AuthEndpoints (Minimal API routes)
  Estapar.Garage.Tests/                       # Testes unitários (xUnit), organizados por camada: Domain/, Application/
```
