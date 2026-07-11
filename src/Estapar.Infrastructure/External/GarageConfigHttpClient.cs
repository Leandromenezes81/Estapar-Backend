using System.Net.Http.Json;
using Estapar.Application.DTOs;
using Estapar.Application.Interfaces;

namespace Estapar.Infrastructure.External;

/// <summary>Reads the garage configuration from the simulator's GET /garage endpoint.</summary>
public sealed class GarageConfigHttpClient : IGarageConfigClient
{
    private readonly HttpClient _httpClient;

    public GarageConfigHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GarageConfigDto> GetGarageConfigurationAsync(CancellationToken cancellationToken = default)
    {
        var config = await _httpClient.GetFromJsonAsync<GarageConfigDto>("/garage", cancellationToken);

        return config ?? throw new InvalidOperationException("The simulator returned an empty garage configuration.");
    }
}
