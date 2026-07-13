# Tasks

Lista de solicitações de modificação para o projeto `Estapar.ParkingManager`.

Nenhuma alteração deve ser executada nas classes até que seja explicitamente solicitado o início das implementações. Este arquivo serve apenas para registrar os pedidos até lá.

## Convenções

- Todos os enumeradores do projeto devem iniciar o índice em `0`.
- Os nomes dos índices devem ser escritos em letras maiúsculas.

## Concluídas

Todas as tarefas abaixo foram implementadas e verificadas (build + testes de domínio OK). A migration foi recriada e aplicada no banco `EstaparParkingManagerDb` em `127.0.0.1,1433` (renomeado de `EstaparDb` — ver tarefa 12), apagado e recriado do zero a cada rodada de correções.

### 1. Ajustar enumerador SpotStatus (já existente)

`SpotStatus` (Estapar.ParkingManager.Domain/Entities/SpotStatus.cs) já existe. Manter a mesma lógica e os índices iniciando em 0, apenas:
   - Reescrever os índices existentes em letras maiúsculas.
   - Incluir o índice `DEACTIVATED`.

Resultado esperado:
   - `AVAILABLE = 0`
   - `OCCUPIED = 1`
   - `DEACTIVATED = 2`

### 2. Criar enumerador EventType

Criar o enumerador `EventType`, que representa o tipo de evento do webhook/processo, iniciando o índice em 0.

   - `ENTRY = 0`
   - `PARKED = 1`
   - `EXIT = 2`

### 3. Adicionar Id à entidade Sector

Avaliação com base no endpoint `GET /garage` (Estapar.Garage): o payload identifica cada setor apenas pelo campo `sector`/`name` (string), sem um identificador numérico. Hoje `Sector` (Estapar.ParkingManager.Domain/Entities/Sector.cs) não possui `Id` — a chave primária é o próprio `Name`.

Como as tarefas 4 e 6 exigem referenciar o setor por `SectorId`, `Sector` precisa passar a ter um `Id` (chave substituta, gerada internamente), mantendo `Name` como identificador natural/único vindo do `/garage`.

Impactos a avaliar:
   - `Sector`: adicionar propriedade `Id`.
   - `ISectorRepository`: adicionar método de busca por `Id` (ex.: `GetByIdAsync`), mantendo `GetByNameAsync` para resolver o setor a partir do nome recebido do `/garage`.
   - `LoadGarageConfigurationUseCase`: ao criar/obter o `Sector`, usar o `Id` gerado (e não mais o `Name`) para preencher `Spot.SectorId` ao criar cada `Spot`.
   - `SectorConfiguration` (EF Core): revisar mapeamento de chave primária (`Name` → `Id`).

### 4. Substituir SectorName por SectorId na entidade Spot

Em `Spot` (Estapar.ParkingManager.Domain/Entities/Spot.cs), a propriedade `SectorName` (`string`) deverá ser substituída por `SectorId`, representando o Id do Setor (tarefa 3). Ajustar também o construtor privado e o método `Create` (que hoje recebem/validam `sectorName`) para refletir a troca.

### 5. Atualizar WebhookEventDto — EventType

Em `WebhookEventDto` (Estapar.ParkingManager.Application/DTO/WebhookEventDto.cs), a propriedade `EventType` (atualmente `string`) deverá passar a ser do tipo do enumerador `EventType` (tarefa 2).

### 6. Renomear Sector para SectorId em WebhookEventDto

Em `WebhookEventDto`, a propriedade `Sector` (atualmente `string?`) deverá ser renomeada para `SectorId`, passando a representar o Id do Setor (tarefa 3).

### 7. Rever Migration

Revisar/recriar a migration do EF Core para refletir todas as alterações acima (Id em `Sector`, `SectorId` em `Spot`, e demais ajustes de mapeamento). Caso a migration incremental não seja viável de forma limpa, considerar apagar todo o banco (LocalDB) e recriar a partir de uma migration nova/consolidada.

## Correções pós-implementação

Bugs encontrados via teste manual dos endpoints, após a implementação das tarefas 1–7. Todos corrigidos, com migration/banco recriados a cada correção.

### 8. Corrigir contagem de sessões ativas por setor (bug de escopo entre garagens)

`HandleEntryUseCase` contava sessões ativas via `CountActiveBySectorAsync(sector.Name, ...)`. Como setores de garagens diferentes podem ter o mesmo `Name` (ex.: "A" existe em todas as 5 garagens, com `Id`s distintos), a contagem somava sessões de todas as garagens com aquele nome, e não apenas da instância de setor em questão — corrompendo o cálculo de lotação (`IsFull`) e do fator de preço dinâmico (`PriceFactorFor`).

Corrigido:
   - `ParkingSession` ganhou `SectorId` (além do `SectorName`, na época).
   - `IParkingSessionRepository.CountActiveBySectorAsync` passou a receber `sectorId` (int).
   - `HandleEntryUseCase` passou a contar e abrir a sessão por `sector.Id`.
   - Índice `(SectorId, ExitTime)` adicionado em `ParkingSessionConfiguration`.

Validado: com um setor "A" de uma garagem lotado/com fator alterado, outro setor "A" de outra garagem permaneceu com fator/lotação corretos e independentes.

### 9. Remover SectorName de ParkingSessions; /revenue e EXIT por SectorId

Identificado que o mesmo tipo de bug da tarefa 8 também afetava `/revenue` (consulta por `SectorName`, somando receita de todas as garagens com setor de mesmo nome) e `HandleExitUseCase` (resolvia o setor via `GetByNameAsync(session.SectorName)`, podendo pegar o preço-base da garagem errada).

Corrigido:
   - `/revenue` (`RevenueRequest`/`RevenueQuery`/`RevenueController`/`GetRevenueUseCase`) passou a receber `sector` como **Id** (int), não mais nome — mesma convenção do `/webhook`. **Isso é uma mudança de contrato público da API.**
   - `SumRevenueAsync` passou a filtrar por `SectorId`.
   - `HandleExitUseCase` passou a resolver o setor via `GetByIdAsync(session.SectorId)`.
   - `ParkingSession.SectorName` removida (não havia mais necessidade — todo o sistema já opera por `SectorId`); `SumRevenueAsync`/contagem de ativos usam exclusivamente `SectorId`.
   - Índice `(SectorName, ExitTime)` removido de `ParkingSessionConfiguration`; `(SectorId, ExitTime)` passou a atender tanto a contagem de ativos quanto a soma de receita. Índice de `LicensePlate` composto com `ExitTime` para cobrir totalmente o filtro de `GetOpenByLicensePlateAsync`.

Validado: dois veículos em setores "A" de garagens diferentes (`SectorId=1` e `SectorId=5`) geraram receitas corretamente segregadas em `/revenue?sector=1` vs `/revenue?sector=5`.

### 10. Escopar a busca de Spot por setor no evento PARKED

Mesma classe de bug identificada também no `HandleParkedUseCase`: a busca da vaga por coordenadas (`GetByCoordinatesAsync(lat, lng)`) não era escopada por setor/garagem, podendo (em tese) casar com uma vaga de coordenadas coincidentes em outra garagem.

Corrigido:
   - `ISpotRepository.GetByCoordinatesAsync` passou a receber `sectorId` (int) além de `lat`/`lng`.
   - `ParkedCommand` ganhou `SectorId`; `WebhookEventDto`/`HandleWebhookEventUseCase` passaram a exigir `sector` (Id) também no evento `PARKED`, não só no `ENTRY`.
   - `HandleParkedUseCase` passou a chamar `GetByCoordinatesAsync(command.SectorId, command.Lat, command.Lng, ...)`.

### 11. Recriar Migration (novamente, após as correções 8–10)

Migration `InitialCreate` recriada e banco apagado/recriado a cada uma das correções acima, para manter o schema consistente com o modelo (coluna `SectorId` em `ParkingSessions`, remoção de `SectorName`, ajuste de índices).

### 12. Renomear o banco de EstaparDb para EstaparParkingManagerDb

Connection string (`appsettings.json`), `AppDbContextFactory` (design-time) e `README.md` atualizados. Banco antigo (`EstaparDb`) removido do servidor após a criação do `EstaparParkingManagerDb`.
