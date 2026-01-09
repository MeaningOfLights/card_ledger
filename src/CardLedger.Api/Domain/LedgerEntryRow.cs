namespace CardLedger.Api.Domain
{
    /// <summary>
    /// Data row representing a ledger entry in the database.
    /// </summary>
    internal sealed class LedgerEntryRow
    {
        /// <summary>
        /// The unique identifier of the ledger entry.
        /// </summary>
        internal Guid LedgerEntryId { get; set; }

        /// <summary>
        /// The unique identifier of the associated card.
        /// </summary>
        internal Guid CardId { get; set; }

        /// <summary>
        /// The idempotency key for the ledger entry.
        /// </summary>
        internal Guid IdempotencyKey { get; set; }

        /// <summary>
        /// The description of the ledger entry.
        /// </summary>
        internal string Description { get; set; } = string.Empty;

        /// <summary>
        /// The transaction date of the ledger entry.
        /// </summary>
        internal DateTimeOffset TransactionDate { get; set; }

        /// <summary>
        /// The amount in USD.
        /// </summary>
        internal decimal AmountInUsd { get; set; }

        /// <summary>
        /// The original amount of the transaction.
        /// </summary>
        internal decimal OriginalAmount { get; set; }

        /// <summary>
        /// The original currency code of the transaction.
        /// </summary>
        internal string OriginalCurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// The type of the ledger entry (e.g., "Purchase", "Payment").
        /// </summary>
        internal string EntryType { get; set; } = string.Empty;

        /// <summary>
        /// The creation timestamp of the ledger entry.
        /// </summary>
        internal DateTime CreatedAt { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public LedgerEntryRow()
        {

        }
    }
}
