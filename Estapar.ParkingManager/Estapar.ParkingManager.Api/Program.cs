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
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + OpenAPI
builder.Services.AddControllers();
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
        ?? throw new InvalidOperationException("Configuration 'GarageApi:BaseUrl' is missing.");
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
