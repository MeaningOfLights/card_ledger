using CardLedger.Api.Application.DTOs;
using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using MediatR;
using MoneyDataType;

namespace CardLedger.Api.Application.Queries;

/// <summary>
/// Handles available balance queries.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetAvailableBalanceHandler"/> class.
/// </remarks>
public sealed class GetAvailableBalanceHandler(ICardLedgerRepository repo, IFxRateProvider fx) : IRequestHandler<GetAvailableBalanceQuery, AvailableBalanceDto>
{
    private readonly ICardLedgerRepository _repo = repo;
    private readonly IFxRateProvider _fx = fx;

    /// <summary>
    /// Returns the available balance in the target currency.
    /// </summary>
    public async Task<AvailableBalanceDto> Handle(GetAvailableBalanceQuery request, CancellationToken ct)
    {
        var currency = (request.TargetCurrency ?? "USD").Trim().ToUpperInvariant();

        var card = await _repo.GetCardAsync(request.CardId, ct)
            ?? throw new KeyNotFoundException("Card not found.");

        var totalSpend = await _repo.GetTotalSpendAsync(request.CardId, ct);

        var creditLimit = card.CreditLimit.WithScale(2);
        var totalSpendScaled = totalSpend.WithScale(2);
        var available = (creditLimit - totalSpendScaled).WithScale(2);
        if (available.Value < 0)
        {
            available = new Money(0m, creditLimit.CurrencyCode).WithScale(2);
        }

        // Apply FX rules: rate_date <= "purchase date" doesn't exist for balance; use today's date per requirement style?
        // For simplicity we use today's date, and keep the same 6-month lookback. Documented in README.
        var fxDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var rate = _fx.GetUsdToCurrencyRate(currency, fxDate);

        var availableTarget = new Money(decimal.Round(available.Value * rate, 2, MidpointRounding.AwayFromZero), currency).WithScale(2);

        return new AvailableBalanceDto(
            CardId: card.CardId,
            CreditLimitFormatted: MoneyFormatting.Format(creditLimit),
            TotalSpendFormatted: MoneyFormatting.Format(totalSpendScaled),
            AvailableFormatted: MoneyFormatting.Format(available),
            TargetCurrency: currency,
            ExchangeRateUsed: rate,
            AvailableInTargetCurrencyFormatted: MoneyFormatting.Format(availableTarget)
        );
    }
}
