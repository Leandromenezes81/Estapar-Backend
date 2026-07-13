using Estapar.ParkingManager.Api.Middleware;
using Estapar.ParkingManager.Api.Registration;
using Estapar.ParkingManager.Application.Registration;
using Estapar.ParkingManager.Infrastructure.BackgroundServices.Registration;
using Estapar.ParkingManager.Infrastructure.Data.Registration;
using System.Globalization;

// Apenas a UI culture (idioma dos recursos de string, ex.: mensagens embutidas
// do DataAnnotations) é alterada aqui — CurrentCulture é deixada intacta para que
// o parsing/formatação de número e data de request/response continue usando o
// padrão invariante (decimais com ponto, datas ISO).
var ptBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentUICulture = ptBr;

// WebApplication.CreateBuilder já carrega, em ordem, appsettings.json,
// appsettings.{EnvironmentName}.json, variáveis de ambiente e argumentos de linha
// de comando — não é necessário adicionar nenhum deles manualmente.
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