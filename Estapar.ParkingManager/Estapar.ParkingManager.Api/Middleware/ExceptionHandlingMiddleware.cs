using System.Net;
using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Domain.Exceptions;

namespace Estapar.ParkingManager.Api.Middleware;

/// <summary>Mapeia exceções de domínio/aplicação para códigos de status HTTP em todas as requisições.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Executa o próximo middleware do pipeline, capturando exceções não tratadas e convertendo-as em uma resposta padronizada.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, message) = MapException(ex);
            _logger.LogWarning(ex, "A requisição {Method} {Path} falhou: {Message}", context.Request.Method, context.Request.Path, ex.Message);

            var response = context.RequestServices.GetRequiredService<Response>();
            response.AddErrorMessages(message);
            await response.GenerateResponse((HttpStatusCode)statusCode);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(response);
        }
    }

    /// <summary>Traduz cada tipo de exceção conhecido para o par (código HTTP, mensagem) correspondente.</summary>
    private static (int StatusCode, string Message) MapException(Exception ex) => ex switch
    {
        GarageFullException e => (StatusCodes.Status409Conflict, e.Message),
        SessionNotFoundException e => (StatusCodes.Status404NotFound, e.Message),
        ArgumentException e => (StatusCodes.Status400BadRequest, e.Message),
        InvalidOperationException e => (StatusCodes.Status400BadRequest, e.Message),
        NotFoundException e => (StatusCodes.Status404NotFound, e.Message),
        InternalServerErrorException e => (StatusCodes.Status500InternalServerError, e.Message),
        _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado.")
    };
}
