# Tasks — Histórico do Projeto Estapar Backend

Registro cronológico das tarefas realizadas desde a criação do projeto até o estado atual, com base no histórico de commits do repositório.

## 2026-07-11 — Fundação da solução e primeira versão funcional

1. **Setup da solução Clean Architecture (.NET 8)** (`07d9ca2`)
   Criação da solução `Estapar.sln` com a separação em camadas (Domain, Application, Infrastructure, Api) seguindo Clean Architecture + DDD.

2. **Implementação da camada de domínio** (`971f6a3`)
   Entidades e regras puras: `Sector`, `Spot`, `ParkingSession`, política de preço dinâmico por ocupação e cálculo de tarifa.

3. **Testes de domínio** (`203a180`)
   Cobertura unitária de `PricingPolicy` e `FeeCalculator` (fatores de ocupação, 30 min grátis, arredondamento de horas).

4. **Camada de aplicação** (`3477c67`)
   Casos de uso (handlers de ENTRY/PARKED/EXIT, consulta de receita, carga da configuração da garagem) e ports (interfaces de repositório e cliente HTTP).

5. **Camada de infraestrutura** (`121c4e1`)
   Persistência com EF Core (DbContext, configurações Fluent API, repositórios) e cliente HTTP para consumir `GET /garage` do simulador.

6. **Camada de API** (`8940717`)
   Controllers (`WebhookController`, `RevenueController`), hosted service de bootstrap da garagem, DI e Swagger/OpenAPI.

7. **Documentação inicial (README)** (`bee89b2`)
   Instruções de setup do projeto.

8. **Configuração de banco de dados** (`5a1a824`)
   Adição da string de conexão ao SQL Server.

## 2026-07-12 — Reorganização, respostas padronizadas e segurança de segredos

9. **Reorganização do ParkingManager** (`bac30cc`)
   Reestruturação do projeto e correção de setores duplicados entre garagens.

10. **Padronização de respostas da API** (`422b79f`)
    Envelopamento das respostas HTTP em um objeto `Response` e tradução das mensagens para pt-BR.

11. **Ignorar arquivos de planejamento/tasks no Git** (`b6b405f`)
    `Tasks.md`/`TASKS.md` passam a ser ignorados; atualização do README.

12. **Remoção de segredos do controle de versão** (`e2548df`)
    Remoção da senha real de `appsettings.json`, migrando para `dotnet user-secrets`.

13. **Ignorar `docs/PLANO.md`** (`7db4755`)
    Arquivo de planejamento interno passa a ser ignorado pelo Git.

14. **Limpeza de referências no README** (`0e829b2`)
    Remoção de referência a `docs/PLANO.md` do README.

15. **Reorganização dos testes** (`7f4d4cd`)
    Testes reorganizados por camada; criação do projeto `Estapar.Garage.Tests`.

## 2026-07-13 — Autenticação, documentação em português e Swagger

16. **Padronização de Registration e Quartz.NET** (`a851b73`)
    Padronização de classes `*Registration` por camada no ParkingManager e integração do Quartz.NET para jobs agendados (recarga periódica da configuração da garagem).

17. **Autenticação JWT e API Key** (`88e789e`)
    Implementação de autenticação JWT independente em ambas as APIs (`POST /auth/token`), com `RevenueController`/CRUD de `/garages` exigindo Bearer token. `GET /garage` protegido por API Key (header `X-Api-Key`) para o consumo serviço-a-serviço entre `Estapar.ParkingManager.Api` e `Estapar.Garage.Api`. `POST /webhook` mantido anônimo (contrato do simulador externo).

18. **Documentação XML em português** (`a81f2da`)
    Adição de comentários `<summary>` em português BR a classes e métodos de todo o projeto (Domain, Application, Infrastructure, ambas APIs e projetos de teste); tradução de comentários remanescentes em inglês.

19. **Swagger UI com autorização JWT/API Key** (`703eb0c`)
    Botão "Authorize" do Swagger habilitado para Bearer JWT nas duas APIs; esquema `ApiKey` dedicado e escopado apenas ao endpoint `GET /garage` (via atributo marcador `ApiKeyAuthAttribute` + `IOperationFilter`, compatível com Swashbuckle). Documentação passo a passo no README para geração de cada segredo (`Jwt:Key`, `Auth:ClientSecret`, `ApiKey`, connection string) via `dotnet user-secrets`, com comandos para PowerShell e Bash/Linux/macOS.

20. **Rotação de segredos expostos**
    Após exposição acidental de valores reais de segredos em conversa, todos os segredos afetados (`Jwt:Key` das duas APIs, `Auth:ClientSecret` das duas APIs, `ApiKey`/`GarageApi:ApiKey` compartilhada) foram regenerados via `dotnet user-secrets set`.

---

## Estado final

- Solução .NET 8 com duas Web APIs independentes (`Estapar.ParkingManager.Api` e `Estapar.Garage.Api`) em Clean Architecture + DDD.
- Autenticação JWT (client credentials, sem base de usuários) protegendo `/revenue` e o CRUD `/garages`; API Key dedicada protegendo o contrato de integração `GET /garage`; `/webhook` anônimo.
- Persistência via EF Core + SQL Server, com jobs Quartz.NET para sincronização periódica.
- Documentação XML completa em português BR, README com guia de configuração de segredos para leigos, e Swagger UI com suporte a autorização.
- Testes unitários organizados por camada (`Estapar.ParkingManager.Tests`, `Estapar.Garage.Tests`), 57 testes passando.
