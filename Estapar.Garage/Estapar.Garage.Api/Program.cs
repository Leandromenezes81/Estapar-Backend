using Estapar.Garage.Api.Application.Interfaces;
using Estapar.Garage.Api.Application.Services;
using Estapar.Garage.Api.Endpoints;
using Estapar.Garage.Api.Infrastructure.Persistence;
using Estapar.Garage.Api.Infrastructure.Persistence.Repositories;
using Estapar.Garage.Api.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Estapar Garage API",
        Version = "v1",
        Description = "Cadastro de garagens, setores e vagas; fonte do contrato GET /garage consumido pelo Estapar.ParkingManager."
    });
});

// Persistence
builder.Services.AddDbContext<GarageDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository / Application service
builder.Services.AddScoped<IGarageRepository, GarageRepository>();
builder.Services.AddScoped<GarageService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGarageEndpoints();

app.Run();
