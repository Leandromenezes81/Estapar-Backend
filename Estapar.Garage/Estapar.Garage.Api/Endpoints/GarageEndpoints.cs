using Estapar.Garage.Api.Application.DTO;
using Estapar.Garage.Api.Application.Services;
using Estapar.Garage.Api.Filters;

namespace Estapar.Garage.Api.Endpoints;

/// <summary>Endpoints de garagens: GET /garage (protegido por API Key) e o CRUD /garages (protegido por JWT).</summary>
public static class GarageEndpoints
{
    /// <summary>Mapeia GET /garage e o grupo de endpoints /garages (CRUD).</summary>
    public static IEndpointRouteBuilder MapGarageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garage", async (GarageService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetGarageConfigurationAsync(cancellationToken)))
            .WithName("GetGarageConfiguration")
            .WithTags("Garage")
            .WithSummary("Todas as garagens (mesmo formato agregado de GET /garages/{id}) — contrato consumido pelo Estapar.ParkingManager. Protegido por API Key (header X-Api-Key), não por JWT.")
            .AddEndpointFilter<ApiKeyEndpointFilter>()
            .WithMetadata(new ApiKeyAuthAttribute())
            .AllowAnonymous()
            .Produces<List<GarageResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        var group = app.MapGroup("/garages").WithTags("Garages").RequireAuthorization();

        group.MapGet("/{id:int}", async (int id, GarageService service, CancellationToken cancellationToken) =>
            {
                var garage = await service.GetByIdAsync(id, cancellationToken);
                return garage is not null ? Results.Ok(garage) : Results.NotFound();
            })
            .WithName("GetGarageById")
            .WithSummary("Busca uma garagem (com setores e vagas) por Id.")
            .Produces<GarageResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateGarageRequest request, GarageService service, CancellationToken cancellationToken) =>
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/garages/{created.Id}", created);
            })
            .WithName("CreateGarage")
            .WithSummary("Cria uma garagem com seus setores e vagas.")
            .Produces<GarageResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:int}", async (int id, UpdateGarageRequest request, GarageService service, CancellationToken cancellationToken) =>
            {
                var updated = await service.UpdateAsync(id, request, cancellationToken);
                return updated ? Results.NoContent() : Results.NotFound();
            })
            .WithName("UpdateGarage")
            .WithSummary("Atualiza nome, setores e vagas de uma garagem (substitui o conjunto de setores/vagas).")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, GarageService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.SoftDeleteAsync(id, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteGarage")
            .WithSummary("Remove (soft delete) uma garagem e seus setores/vagas.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
