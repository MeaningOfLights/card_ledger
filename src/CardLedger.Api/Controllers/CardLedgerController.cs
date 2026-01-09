using CardLedger.Api.Application.Commands;
using CardLedger.Api.Application.DTOs;
using CardLedger.Api.Application.Queries;
using CardLedger.Api.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CardLedger.Api.Controllers;

/// <summary>
/// HTTP endpoints for card and purchase operations.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="mediator">Mediatr by Jimmy Bogard</param>
[ApiController]
[Route("")]
public sealed class CardLedgerController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Creates a card.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The CancellationToken.</param>
    /// <returns>An IActionResult.</returns>
    [HttpPost("cards")]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCard([FromBody] CreateCardRequest request, CancellationToken ct)
    {
        var dto = await _mediator.Send(new CreateCardCommand
        {
            CardNumber = request.CardNumber,
            CreditLimit = request.CreditLimit,
            CurrencyCode = request.CurrencyCode
        }, ct);
        return CreatedAtAction(nameof(GetAvailableBalance), new { cardId = dto.CardId }, dto);
    }

    /// <summary>
    /// Creates a purchase for the given card.
    /// </summary>
    /// <param name="cardId">The ID of the card.</param>
    /// <param name="request">The request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created purchase ID.</returns>
    [HttpPost("cards/{cardId:guid}/purchases")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreatePurchase([FromRoute] Guid cardId, [FromBody] CreatePurchaseRequest request, CancellationToken ct)
    {
        // Idempotency key: prefer header, else generate.
        // Header name intentionally simple for interviewers.
        var idemHeader = Request.Headers.TryGetValue("Idempotency-Key", out var values) ? values.ToString() : null;
        Guid idempotencyKey = Guid.TryParse(idemHeader, out var parsed) ? parsed : UuidV7.New();

        var purchaseId = await _mediator.Send(new CreatePurchaseCommand
        {
            CardId = cardId,
            IdempotencyKey = idempotencyKey,
            Description = request.Description,
            TransactionDate = request.TransactionDate,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode
        }, ct);

        return Created($"/purchases/{purchaseId}", new { purchaseId, idempotencyKey });
    }

    /// <summary>
    /// Gets a purchase by ID, optionally converting to target currency.
    /// </summary>
    /// <param name="purchaseId">The ID of the purchase.</param>
    /// <param name="currency">The target currency code (default: USD).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The purchase details.</returns>
    [HttpGet("purchases/{purchaseId:guid}")]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchase([FromRoute] Guid purchaseId, [FromQuery] string currency = "USD", CancellationToken ct = default)
    {
        var dto = await _mediator.Send(new GetPurchaseQuery(purchaseId, currency), ct);
        return Ok(dto);
    }

    /// <summary>
    /// Gets the available balance for a card in the specified currency.
    /// </summary>
    /// <param name="cardId">The ID of the card.</param>
    /// <param name="currency">The target currency code (default: USD).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The available balance details.</returns>
    [HttpGet("cards/{cardId:guid}/available-balance")]
    [ProducesResponseType(typeof(AvailableBalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableBalance([FromRoute] Guid cardId, [FromQuery] string currency = "USD", CancellationToken ct = default)
    {
        var dto = await _mediator.Send(new GetAvailableBalanceQuery(cardId, currency), ct);
        return Ok(dto);
    }
}

