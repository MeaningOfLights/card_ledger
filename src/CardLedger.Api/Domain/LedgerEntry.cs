using MoneyDataType;

namespace CardLedger.Api.Domain;

/// <summary>
/// Represents a ledger entry for a card transaction.
/// </summary>
/// <remarks>
/// Constructor.
/// </remarks>
/// <param name="ledgerEntryId">The ledgerEntryId.</param>
/// <param name="cardId">The Card Id.</param>
/// <param name="idempotencyKey">The idempotencyKey.</param>
/// <param name="description">The description.</param>
/// <param name="transactionDate">The transactionDate.</param>
/// <param name="originalAmount">The originalAmount.</param>
/// <param name="amount">The amount.</param>
/// <param name="entryType">The entryType.</param>
/// <param name="createdAt">The createdAt.</param>
public sealed class LedgerEntry(Guid ledgerEntryId, Guid cardId, Guid idempotencyKey, string description, DateTimeOffset transactionDate, Money originalAmount, Money amount, string entryType, DateTimeOffset createdAt)
{

    /// <summary>
    /// The unique identifier of the ledger entry.
    /// </summary>
    public Guid LedgerEntryId { get; set; } = ledgerEntryId;

    /// <summary>
    /// The unique identifier of the associated card.
    /// </summary>
    public Guid CardId { get; set; } = cardId;

    /// <summary>
    /// Gets or sets the idempotency key.
    /// </summary>
    public Guid IdempotencyKey { get; set; } = idempotencyKey;

    /// <summary>
    /// The idempotency key for the ledger entry.
    /// </summary>
    public string Description { get; set; } = description;

    /// <summary>
    /// The transaction date of the ledger entry.
    /// </summary>
    public DateTimeOffset TransactionDate { get; set; } = transactionDate;

    /// <summary>
    /// The original amount of the transaction.
    /// </summary>
    public Money OriginalAmount { get; set; } = originalAmount;

    /// <summary>
    /// The amount of the transaction.
    /// </summary>
    public Money Amount { get; set; } = amount;

    /// <summary>
    /// The type of the ledger entry (e.g., "Purchase", "Payment").
    /// </summary>
    public string EntryType { get; set; } = entryType;

    /// <summary>
    /// The creation timestamp of the ledger entry.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
}

