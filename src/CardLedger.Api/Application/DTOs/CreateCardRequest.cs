namespace CardLedger.Api.Application.DTOs;

/// <summary>
/// DTO request to create a card.
/// </summary>
public sealed class CreateCardRequest
{
    /// <summary>
    /// Card number as a 16-digit numeric string.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Credit limit amount.
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Currency code for the credit limit - restricted to USD per specification for now.
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";
}
