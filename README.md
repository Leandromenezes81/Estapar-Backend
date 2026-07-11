# Estapar Backend

Backend de gerenciamento de estacionamento (vagas, entrada/saída de veículos e receita), implementado como teste técnico da Estapar. Detalhes de regras de negócio e decisões de arquitetura em [`docs/PLANO.md`](docs/PLANO.md); progresso da implementação em [`Tasks.md`](Tasks.md).

## Stack

- .NET 8 (ASP.NET Core WebAPI)
- Entity Framework Core + SQL Server (LocalDB)
- Swagger / OpenAPI (Swashbuckle)
- xUnit + FluentAssertions (testes de domínio)
- Arquitetura Limpa + DDD (`Domain` → `Application` → `Infrastructure`/`Api`)

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (instalado com o Visual Studio ou via [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb))
- Simulador da Estapar rodando localmente (expõe `GET /garage` e recebe eventos de `POST /webhook` da nossa API)

## Configuração

As configurações ficam em `src/Estapar.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EstaparDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Simulator": {
    "BaseUrl": "http://localhost:3000"
  }
}
```

Ajuste `Simulator:BaseUrl` para o endereço real do simulador antes de rodar (ou via variável de ambiente `Simulator__BaseUrl`).

## Como rodar

1. **Restaurar dependências**
   ```
   dotnet restore
   ```

2. **Aplicar as migrations no LocalDB** (cria o banco `EstaparDb` e as tabelas):
   ```
   dotnet ef database update --project src/Estapar.Infrastructure
   ```
   > Requer a ferramenta `dotnet-ef`, já registrada como tool local no repositório (`dotnet tool restore` se necessário).

3. **Subir o simulador** da Estapar apontando para esta API (ex.: `http://localhost:5080/webhook` como destino dos eventos).

4. **Rodar a API**:
   ```
   dotnet run --project src/Estapar.Api
   ```
   Na inicialização, a API busca `GET /garage` no simulador e popula `Sectors`/`Spots` no banco (idempotente — pode reiniciar sem duplicar dados). Se o simulador não estiver acessível, a aplicação falha ao subir (dependência obrigatória, conforme o requisito "ao iniciar a solução, busque e armazene os dados da garagem").

5. **Swagger UI** (ambiente Development): `http://localhost:5080/swagger`.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/webhook` | Recebe eventos `ENTRY`, `PARKED`, `EXIT` do simulador. Retorna `200`. |
| `GET` | `/revenue` | Receita total por setor e data. Aceita `{ "date": "2025-01-01", "sector": "A" }` no corpo (conforme o spec) ou `?date=2025-01-01&sector=A` na query string. |

## Testes

```
dotnet test tests/Estapar.Domain.Tests
```

Cobre `PricingPolicy` (limiares de ocupação 25/50/75/100%) e `FeeCalculator` (30 min grátis, arredondamento de hora, aplicação do fator de preço).

## Suposições assumidas fora da spec

- O payload de `ENTRY` do `.docx` não inclui `sector`, mas o fator de preço dinâmico precisa ser travado no momento da entrada (por setor). Assumimos que o simulador envia um campo adicional `"sector"` no evento `ENTRY`. Caso o simulador real não envie esse campo, o `HandleWebhookEventUseCase` rejeita o evento com `400 Bad Request` — nesse cenário, a resolução do setor precisaria migrar para o evento `PARKED` (ver nota em `docs/PLANO.md`).
- `GET /revenue` com corpo em uma requisição GET é atípico; a API aceita tanto o body quanto query string para maior compatibilidade com clientes/ferramentas que não enviam corpo em GET.

## Estrutura do projeto

```
src/
  Estapar.Domain/          # Entidades, VOs, regras de negócio puras
  Estapar.Application/     # Casos de uso, DTOs, interfaces (ports)
  Estapar.Infrastructure/  # EF Core, repositórios, cliente HTTP do simulador
  Estapar.Api/              # Controllers, DI, Swagger, hosted service de bootstrap
tests/
  Estapar.Domain.Tests/    # Testes unitários de domínio (xUnit)
```
