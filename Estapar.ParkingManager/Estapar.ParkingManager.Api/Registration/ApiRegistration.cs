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
                // Parâmetros de tipo de referência não anuláveis (ex.: "WebhookEventDto dto")
                // recebem um [Required] implícito do [ApiController], cuja mensagem padrão
                // ("The {0} field is required.") vem do System.ComponentModel.DataAnnotations —
                // este ambiente não tem um recurso satélite pt-BR para ela, e ela duplica a
                // mensagem (já em pt-BR) de MissingRequestBodyRequiredValueAccessor abaixo para
                // um corpo vazio, por isso é suprimida em vez de ficar em inglês.
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

                // As mensagens de model-binding embutidas no framework (corpo ausente, campo
                // obrigatório ausente, tipo incorreto etc.) são literais fixos em inglês, não
                // recursos localizados — precisam ser sobrescritas explicitamente para ficarem
                // em pt-BR.
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
                // [ApiController] interrompe o fluxo com seu próprio ValidationProblemDetails
                // quando o model state é inválido (ex.: JSON malformado, campos obrigatórios
                // ausentes) antes mesmo da action ou do middleware de exceção rodarem — envolve
                // essa resposta em Response também, para que todo caminho de erro desta API
                // tenha o mesmo formato.
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

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Informe apenas o token JWT (sem o prefixo 'Bearer '), obtido em POST /auth/token."
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
