namespace Estapar.Garage.Api.Filters;

/// <summary>Marca um endpoint minimal API como protegido por API Key, para que o Swagger exiba o esquema correto (ver ApiKeySecurityOperationFilter).</summary>
public sealed class ApiKeyAuthAttribute : Attribute;
