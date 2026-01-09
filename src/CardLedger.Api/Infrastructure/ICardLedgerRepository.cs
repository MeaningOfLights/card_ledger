using CardLedger.Api.Domain;
using Dapper;
using MoneyDataType;
using Npgsql;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// Data access operations for cards and ledger entries.
/// </summary>
public interface ICardLedgerRepository
{
    /// <summary>
    /// Creates the card asynchronous.
    /// </summary>
    /// <param name="cardNumber">The card number.</param>
    /// <param name="creditLimit">The credit limit.</param>
    /// <param name="ct">The Cancellation Token.</param>
    /// <returns></returns>
    Task<Card> CreateCardAsync(string cardNumber, Money creditLimit, CancellationToken ct);

    /// <summary>
    /// Gets the card asynchronous.
    /// </summary>
    /// <param name="cardId">The card identifier.</param>
    /// <param name="ct">The Cancellation Token.</param>
    /// <returns></returns>
    Task<Card?> GetCardAsync(Guid cardId, CancellationToken ct);

    /// <summary>
    /// Appends a PURCHASE ledger entry exactly-once using (cardId, idempotencyKey) uniqueness.
    /// If an entry already exists for the idempotency key, returns the existing entry.
    /// Also updates spend projection transactionally.
    /// </summary>
    Task<LedgerEntry> AppendPurchaseAsync(
        Guid cardId,
        Guid idempotencyKey,
        string description,
        DateTimeOffset transactionDate,
        Money originalAmount,
        Money amount,
        CancellationToken ct);

    /// <summary>
    /// Gets the purchase asynchronous.
    /// </summary>
    /// <param name="purchaseId">The purchase identifier.</param>
    /// <param name="ct">The Cancellation Token.</param>
    /// <returns></returns>
    Task<LedgerEntry?> GetPurchaseAsync(Guid purchaseId, CancellationToken ct);

    /// <summary>
    /// Gets the total spend asynchronous.
    /// </summary>
    /// <param name="cardId">The card identifier.</param>
    /// <param name="ct">The Cancellation Token.</param>
    /// <returns></returns>
    Task<Money> GetTotalSpendAsync(Guid cardId, CancellationToken ct);
}
