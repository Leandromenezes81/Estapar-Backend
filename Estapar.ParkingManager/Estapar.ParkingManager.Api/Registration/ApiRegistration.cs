using System.Net;
using Estapar.ParkingManager.Application.DTO;
using Microsoft.AspNetCore.Mvc;

namespace Estapar.ParkingManager.Api.Registration;

/// <summary>
/// Classe estática para DI da própria camada Api (controllers, Swagger, model binding)
/// </summary>
public static class ApiRegistration
{
    /// <summary>
    /// Método de extensão para registro dos serviços da Api no contexto de DI
    /// </summary>
    /// <param name="services">IServiceCollection Interface</param>
    /// <returns>IServiceCollection para encadeamento.</returns>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers(options =>
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

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Estapar Parking API",
                Version = "v1",
                Description = "Backend de gerenciamento de estacionamento: vagas, entrada/saída de veículos e receita."
            });
        });

        return services;
    }
}
