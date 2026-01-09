namespace CardLedger.Api.Application.DTOs;

/// <summary>
/// DTO representing available balance details.
/// </summary>
public sealed record AvailableBalanceDto(
    Guid CardId,
    string CreditLimitFormatted,
    string TotalSpendFormatted,
    string AvailableFormatted,
    string TargetCurrency,
    decimal ExchangeRateUsed,
    string AvailableInTargetCurrencyFormatted
);
