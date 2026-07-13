using System.Net.Http.Json;
using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.Interfaces;

namespace Estapar.ParkingManager.Infrastructure.Data.External;

/// <summary>Reads the garage configuration from the Estapar.Garage.Api's GET /garage endpoint.</summary>
public sealed class GarageConfigHttpClient : IGarageConfigClient
{
    private readonly HttpClient _httpClient;

    public GarageConfigHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GarageConfigDto>> GetGarageConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = await _httpClient.GetFromJsonAsync<List<GarageConfigDto>>("/garage", cancellationToken);

        return config ?? throw new InvalidOperationException("The Garage API returned an empty garage configuration.");
    }
}
