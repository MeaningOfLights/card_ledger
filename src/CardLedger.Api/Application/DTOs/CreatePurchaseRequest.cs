namespace CardLedger.Api.Application.DTOs;

/// <summary>
/// DTO request to create a purchase.
/// </summary>
public sealed class CreatePurchaseRequest
{
    /// <summary>
    /// Description of the purchase.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date and time of the transaction.
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
}
