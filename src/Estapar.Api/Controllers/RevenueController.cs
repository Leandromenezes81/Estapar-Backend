using System.Text.Json;
using Estapar.Application.DTOs;
using Estapar.Application.UseCases.Revenue;
using Microsoft.AspNetCore.Mvc;

namespace Estapar.Api.Controllers;

[ApiController]
[Route("revenue")]
public sealed class RevenueController : ControllerBase
{
    private readonly GetRevenueUseCase _useCase;

    public RevenueController(GetRevenueUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>
    /// Returns the total revenue for a sector on a given date. The test spec
    /// shows GET with a JSON body (`{ "date": "...", "sector": "..." }`); we
    /// also accept `?date=&amp;sector=` query parameters for clients that
    /// can't send a body on GET.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<RevenueResponse>> Get(
        [FromQuery] string? sector,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (Request.ContentLength is > 0)
        {
            var body = await JsonSerializer.DeserializeAsync<RevenueRequest>(Request.Body, cancellationToken: cancellationToken);
            sector ??= body?.Sector;
            date ??= body?.Date;
        }

        if (string.IsNullOrWhiteSpace(sector) || date is null)
            return BadRequest("Both 'sector' and 'date' are required, either as a JSON body or as query parameters.");

        var query = new RevenueQuery(sector, date.Value);
        var money = await _useCase.HandleAsync(query, cancellationToken);

        return Ok(new RevenueResponse(money.Amount, money.Currency, DateTime.UtcNow));
    }
}
