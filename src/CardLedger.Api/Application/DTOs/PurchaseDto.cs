namespace CardLedger.Api.Application.DTOs;

/// <summary>
/// DTO representing a purchase.
/// </summary>
public sealed record PurchaseDto(
    Guid PurchaseId,
    Guid CardId,
    string Description,
    DateTimeOffset TransactionDate,
    string OriginalAmountFormatted,
    string OriginalCurrency,
    string TargetCurrency,
    decimal ExchangeRateUsed,
    string ConvertedAmountFormatted
);
