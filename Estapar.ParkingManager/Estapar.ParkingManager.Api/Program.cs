using Estapar.ParkingManager.Api.Middleware;
using Estapar.ParkingManager.Api.Registration;
using Estapar.ParkingManager.Application.Registration;
using Estapar.ParkingManager.Infrastructure.BackgroundServices.Registration;
using Estapar.ParkingManager.Infrastructure.Data.Registration;
using System.Globalization;

// Only the UI culture (resource string language, e.g. built-in DataAnnotations
// messages) changes here — CurrentCulture is left untouched so request/response
// number and date parsing/formatting keeps using invariant (dot decimals, ISO dates).
var ptBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = ptBr;

// WebApplication.CreateBuilder already loads, in order, appsettings.json,
// appsettings.{EnvironmentName}.json, environment variables and command-line
// args — no need to add any of these manually.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddInfrastructureData(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddBackgroundServices(builder.Configuration);

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

app.MapControllers();

app.Run();