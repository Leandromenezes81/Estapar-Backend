using Estapar.Garage.Api.Application.DTOs;
using Estapar.Garage.Api.Application.Services;

namespace Estapar.Garage.Api.Endpoints;

public static class GarageEndpoints
{
    public static IEndpointRouteBuilder MapGarageEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/garage", async (GarageService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetGarageConfigurationAsync(cancellationToken)))
            .WithName("GetGarageConfiguration")
            .WithTags("Garage")
            .WithSummary("Todas as garagens (mesmo formato agregado de GET /garages/{id}) — contrato consumido pelo Estapar.ParkingManager.")
            .Produces<List<GarageResponse>>(StatusCodes.Status200OK);

        var group = app.MapGroup("/garages").WithTags("Garages");

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
