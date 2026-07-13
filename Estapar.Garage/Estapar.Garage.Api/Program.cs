using Estapar.Garage.Api.Application.Interfaces;
using Estapar.Garage.Api.Application.Services;
using Estapar.Garage.Api.Auth;
using Estapar.Garage.Api.Endpoints;
using Estapar.Garage.Api.Infrastructure.Persistence;
using Estapar.Garage.Api.Infrastructure.Persistence.Repositories;
using Estapar.Garage.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// JWT authentication (protege o CRUD /garages) + emissão de token via /auth/token.
// GET /garage usa API Key (ApiKeyEndpointFilter), não JWT — ver GarageEndpoints.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("A configuração 'Jwt:Key' não foi definida.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGarageEndpoints();

app.Run();
