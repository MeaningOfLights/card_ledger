namespace CardLedger.Api.Domain
{
    /// <summary>
    /// Data row representing a card in the database.
    /// </summary>
    /// <remarks>
    /// Constructor.
    /// </remarks>
    /// <param name="cardId">The Card Id.</param>
    /// <param name="cardNumber">The cardNumber.</param>
    /// <param name="creditLimit">The creditLimit.</param>
    /// <param name="currencyCode">The currencyCode.</param>
    /// <param name="createdAt">The createdAt.</param>
    internal sealed class CardRow(Guid cardId, string cardNumber, decimal creditLimit, string currencyCode, DateTime createdAt)
    {
        /// <summary>
        /// The unique identifier of the card.
        /// </summary>
        internal Guid CardId { get; set; } = cardId;

        /// <summary>
        /// The card number.
        /// </summary>
        internal string CardNumber { get; set; } = cardNumber;

        /// <summary>
        /// The credit limit amount.
        /// </summary>
        internal decimal CreditLimit { get; set; } = creditLimit;

        /// <summary>
        /// The currency code for the credit limit.
        /// </summary>
        internal string CurrencyCode { get; set; } = currencyCode;

        /// <summary>
        /// The creation timestamp.
        /// </summary>
        internal DateTime CreatedAt { get; set; } = createdAt;
    }
}

