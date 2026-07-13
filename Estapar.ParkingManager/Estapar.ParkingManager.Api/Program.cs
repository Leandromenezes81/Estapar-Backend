using Estapar.ParkingManager.Api.HostedServices;
using Estapar.ParkingManager.Api.Middleware;
using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.Interfaces;
using Estapar.ParkingManager.Application.UseCases.Entry;
using Estapar.ParkingManager.Application.UseCases.Exit;
using Estapar.ParkingManager.Application.UseCases.GarageBootstrap;
using Estapar.ParkingManager.Application.UseCases.Parked;
using Estapar.ParkingManager.Application.UseCases.Revenue;
using Estapar.ParkingManager.Application.UseCases.Webhook;
using Estapar.ParkingManager.Infrastructure.Data.External;
using Estapar.ParkingManager.Infrastructure.Data.Persistence;
using Estapar.ParkingManager.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;

// Only the UI culture (resource string language, e.g. built-in DataAnnotations
// messages) changes here — CurrentCulture is left untouched so request/response
// number and date parsing/formatting keeps using invariant (dot decimals, ISO dates).
var ptBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = ptBr;

var builder = WebApplication.CreateBuilder(args);

// Controllers + OpenAPI
builder.Services.AddControllers(options =>
    {
        // Non-nullable reference type parameters (e.g. "WebhookEventDto dto") get an
        // implicit [Required] from [ApiController], whose default message ("The {0}
        // field is required.") comes from System.ComponentModel.DataAnnotations —
        // this environment has no pt-BR satellite resource for it, and it duplicates
        // the (already pt-BR) MissingRequestBodyRequiredValueAccessor message below
        // for an empty body, so it's suppressed rather than left in English.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

        // The framework's built-in model-binding messages (missing body, missing
        // required field, wrong type, etc.) are hard-coded English literals, not
        // localized resources — they must be overridden explicitly to get pt-BR.
        var provider = options.ModelBindingMessageProvider;
        provider.SetMissingBindRequiredValueAccessor(name => $"O campo '{name}' é obrigatório.");
        provider.SetMissingKeyOrValueAccessor(() => "É necessário informar um valor.");
        provider.SetMissingRequestBodyRequiredValueAccessor(() => "Um corpo de requisição não vazio é obrigatório.");
        provider.SetValueMustNotBeNullAccessor(value => $"O valor '{value}' é inválido.");
        provider.SetAttemptedValueIsInvalidAccessor((value, name) => $"O valor '{value}' não é válido para '{name}'.");
        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"O valor '{value}' não é válido.");
        provider.SetUnknownValueIsInvalidAccessor(name => $"O valor informado para '{name}' não é válido.");
        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
        provider.SetValueIsInvalidAccessor(value => $"O valor '{value}' é inválido.");
        provider.SetValueMustBeANumberAccessor(name => $"O campo '{name}' deve ser um número.");
        provider.SetNonPropertyValueMustBeANumberAccessor(() => "O campo deve ser um número.");
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // [ApiController] short-circuits with its own ValidationProblemDetails on invalid
        // model state (e.g. malformed JSON, missing required fields) before the action or
        // the exception middleware ever runs — wrap that response in Response too, so every
        // error path from this API is shaped consistently.
        options.InvalidModelStateResponseFactory = context =>
        {
            var response = context.HttpContext.RequestServices.GetRequiredService<Response>();
            response.SetStatusCode(HttpStatusCode.BadRequest);

            foreach (var error in context.ModelState.Values.SelectMany(v => v.Errors))
            {
                response.AddErrorMessages(string.IsNullOrEmpty(error.ErrorMessage)
                    ? error.Exception?.Message ?? "Requisição inválida."
                    : error.ErrorMessage);
            }

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Estapar Parking API",
        Version = "v1",
        Description = "Backend de gerenciamento de estacionamento: vagas, entrada/saída de veículos e receita."
    });
});

// Persistence
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories / Unit of Work
builder.Services.AddScoped<ISectorRepository, SectorRepository>();
builder.Services.AddScoped<ISpotRepository, SpotRepository>();
builder.Services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Estapar.Garage.Api HTTP client — source of the GET /garage configuration
builder.Services.AddHttpClient<IGarageConfigClient, GarageConfigHttpClient>(client =>
{
    var baseUrl = builder.Configuration["GarageApi:BaseUrl"]
        ?? throw new InvalidOperationException("A configuração 'GarageApi:BaseUrl' não foi definida.");
    client.BaseAddress = new Uri(baseUrl);
});

// Use cases
builder.Services.AddScoped<HandleEntryUseCase>();
builder.Services.AddScoped<HandleParkedUseCase>();
builder.Services.AddScoped<HandleExitUseCase>();
builder.Services.AddScoped<HandleWebhookEventUseCase>();
builder.Services.AddScoped<GetRevenueUseCase>();
builder.Services.AddScoped<LoadGarageConfigurationUseCase>();

// Fetches and stores the garage/spot configuration on startup
builder.Services.AddHostedService<GarageBootstrapHostedService>();

builder.Services.AddScoped<Response>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
