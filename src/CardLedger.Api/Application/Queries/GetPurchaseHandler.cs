using CardLedger.Api.Application.DTOs;
using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using MediatR;
using MoneyDataType;

namespace CardLedger.Api.Application.Queries;

/// <summary>
/// Handles purchase retrieval queries.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPurchaseHandler"/> class.
/// </remarks>
public sealed class GetPurchaseHandler(ICardLedgerRepository repo, IFxRateProvider fx) : IRequestHandler<GetPurchaseQuery, PurchaseDto>
{
    private readonly ICardLedgerRepository _repo = repo;
    private readonly IFxRateProvider _fx = fx;

    /// <summary>
    /// Returns a purchase converted to the target currency.
    /// </summary>
    public async Task<PurchaseDto> Handle(GetPurchaseQuery request, CancellationToken ct)
    {
        var currency = (request.TargetCurrency ?? "USD").Trim().ToUpperInvariant();

        var purchase = await _repo.GetPurchaseAsync(request.PurchaseId, ct)
            ?? throw new KeyNotFoundException("Purchase not found.");

        var originalAmount = purchase.OriginalAmount.WithScale(2);
        var amountInUsd = purchase.Amount.WithScale(2);
        decimal rate;
        Money converted;
        if (string.Equals(originalAmount.CurrencyCode, currency, StringComparison.OrdinalIgnoreCase))
        {
            rate = 1m;
            converted = originalAmount;
        }
        else if (string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase))
        {
            rate = 1m;
            converted = amountInUsd;
        }
        else
        {
            rate = _fx.GetUsdToCurrencyRate(currency, DateOnly.FromDateTime(purchase.TransactionDate.UtcDateTime));
            converted = new Money(decimal.Round(amountInUsd.Value * rate, 2, MidpointRounding.AwayFromZero), currency);
        }

        return new PurchaseDto(
            PurchaseId: purchase.LedgerEntryId,
            CardId: purchase.CardId,
            Description: purchase.Description,
            TransactionDate: purchase.TransactionDate,
            OriginalAmountFormatted: MoneyFormatting.Format(originalAmount),
            OriginalCurrency: originalAmount.CurrencyCode,
            TargetCurrency: currency,
            ExchangeRateUsed: rate,
            ConvertedAmountFormatted: MoneyFormatting.Format(converted.WithScale(2))
        );
    }
}
