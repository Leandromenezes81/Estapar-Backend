using System.Net;
using Estapar.ParkingManager.Application.DTO;
using Estapar.ParkingManager.Application.UseCases.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace Estapar.ParkingManager.Api.Controllers;

[ApiController]
[Route("webhook")]
public sealed class WebhookController : ControllerBase
{
    private readonly HandleWebhookEventUseCase _useCase;
    private readonly Response _response;

    public WebhookController(HandleWebhookEventUseCase useCase, Response response)
    {
        _useCase = useCase;
        _response = response;
    }

    /// <summary>Recebe eventos ENTRY, PARKED e EXIT enviados pelo simulador.</summary>
    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] WebhookEventDto dto, CancellationToken cancellationToken)
    {
        await _useCase.HandleAsync(dto, cancellationToken);

        var response = await _response.GenerateResponse(HttpStatusCode.OK, $"Evento '{dto.EventType}' processado com sucesso.");
        return Ok(response);
    }
}
