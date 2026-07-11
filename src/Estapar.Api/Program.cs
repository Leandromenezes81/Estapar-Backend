using Estapar.Api.HostedServices;
using Estapar.Api.Middleware;
using Estapar.Application.Interfaces;
using Estapar.Application.UseCases.Entry;
using Estapar.Application.UseCases.Exit;
using Estapar.Application.UseCases.GarageBootstrap;
using Estapar.Application.UseCases.Parked;
using Estapar.Application.UseCases.Revenue;
using Estapar.Application.UseCases.Webhook;
using Estapar.Infrastructure.External;
using Estapar.Infrastructure.Persistence;
using Estapar.Infrastructure.Repositories;
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

// Simulator HTTP client
builder.Services.AddHttpClient<IGarageConfigClient, GarageConfigHttpClient>(client =>
{
    var baseUrl = builder.Configuration["Simulator:BaseUrl"]
        ?? throw new InvalidOperationException("Configuration 'Simulator:BaseUrl' is missing.");
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
