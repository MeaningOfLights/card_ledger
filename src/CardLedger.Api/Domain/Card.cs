using MoneyDataType;

namespace CardLedger.Api.Domain;

/// <summary>
/// Represents a credit card.
/// </summary>
public sealed class Card {

    /// <summary>
    /// The unique identifier of the card.
    /// </summary>
    public Guid CardId { get; set; }

    /// <summary>
    /// The card number.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// The credit limit of the card.
    /// </summary>
    public Money CreditLimit { get; set; }

    /// <summary>
    /// The creation timestamp of the card.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public Card()
    {
    }
}

