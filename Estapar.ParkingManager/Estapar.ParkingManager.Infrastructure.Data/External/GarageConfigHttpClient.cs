using System.Net.Http.Json;
using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.Interfaces;

namespace Estapar.ParkingManager.Infrastructure.Data.External;

/// <summary>Lê a configuração de garagens a partir do endpoint GET /garage da Estapar.Garage.Api.</summary>
public sealed class GarageConfigHttpClient : IGarageConfigClient
{
    private readonly HttpClient _httpClient;

    public GarageConfigHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>Obtém a configuração completa de todas as garagens, lançando exceção se a resposta vier vazia.</summary>
    public async Task<List<GarageConfigDto>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = await _httpClient.GetFromJsonAsync<List<GarageConfigDto>>("/garage", cancellationToken);

        return config ?? throw new InvalidOperationException("A API da garagem retornou uma configuração vazia.");
    }
}
