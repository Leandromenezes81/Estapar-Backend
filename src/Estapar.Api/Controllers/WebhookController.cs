using Estapar.Application.DTOs;
using Estapar.Application.UseCases.Webhook;
using Microsoft.AspNetCore.Mvc;

namespace Estapar.Api.Controllers;

[ApiController]
[Route("webhook")]
public sealed class WebhookController : ControllerBase
{
    private readonly HandleWebhookEventUseCase _useCase;

    public WebhookController(HandleWebhookEventUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>Receives ENTRY, PARKED and EXIT events from the simulator.</summary>
    [HttpPost]
    public async Task<IActionResult> Handle([FromBody] WebhookEventDto dto, CancellationToken cancellationToken)
    {
        await _useCase.HandleAsync(dto, cancellationToken);
        return Ok();
    }
}
