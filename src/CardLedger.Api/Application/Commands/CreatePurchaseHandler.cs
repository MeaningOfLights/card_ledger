using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using MediatR;
using MoneyDataType;

namespace CardLedger.Api.Application.Commands;

/// <summary>
/// Handles creating purchases.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreatePurchaseHandler"/> class.
/// </remarks>
public sealed class CreatePurchaseHandler(ICardLedgerRepository repo, IFxRateProvider fx) : IRequestHandler<CreatePurchaseCommand, Guid>
{
    private readonly ICardLedgerRepository _repo = repo;
    private readonly IFxRateProvider _fx = fx;

    /// <summary>
    /// Creates a purchase after validating inputs.
    /// </summary>
    public async Task<Guid> Handle(CreatePurchaseCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Length > 50)
        {
            throw new ArgumentException("Description must be 1..50 characters.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Purchase amount must be positive.");
        }

        var originalAmount = Money.Create(request.Amount, request.CurrencyCode).WithScale(2);
        var amount = originalAmount;
        if (!string.Equals(originalAmount.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase))
        {
            var rateDate = DateOnly.FromDateTime(request.TransactionDate.UtcDateTime);
            decimal usdToCurrency;
            try
            {
                // Throws an error if the target FX rate can't be found relative to the date.
                usdToCurrency = _fx.GetUsdToCurrencyRate(originalAmount.CurrencyCode, rateDate);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    $"Purchase cannot be converted to USD. No FX rate available for {originalAmount.CurrencyCode} on or before {rateDate:yyyy-MM-dd} within the last 6 months.",
                    ex);
            }
            var usdValue = originalAmount.Value / usdToCurrency;
            amount = Money.Create(usdValue, "USD", null).WithScale(2);
        }

        var entry = await _repo.AppendPurchaseAsync(
            request.CardId,
            request.IdempotencyKey,
            request.Description,
            request.TransactionDate,
            originalAmount,
            amount,
            ct);

        return entry.LedgerEntryId;
    }
}
