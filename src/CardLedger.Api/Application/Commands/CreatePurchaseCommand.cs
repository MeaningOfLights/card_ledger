using MediatR;

namespace CardLedger.Api.Application.Commands;

/// <summary>
/// Command to create a purchase.
/// </summary>
public class CreatePurchaseCommand : IRequest<Guid>
{
    /// <summary>
    /// ID of the card for the purchase.
    /// </summary>
    public Guid CardId { get; set; }

    /// <summary>
    /// Idempotency key to prevent duplicate purchases.
    /// </summary>
    public Guid IdempotencyKey { get; set; }

    /// <summary>
    /// Description of the purchase.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date of the purchase.
    /// </summary>
    public DateTimeOffset TransactionDate { get; set; }

    /// <summary>
    /// Amount of the purchase.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code of the purchase amount.
    /// </summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// Region of the purchase.
    /// </summary>
    public CreatePurchaseCommand() { }
} 
