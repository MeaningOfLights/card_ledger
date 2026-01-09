using CardLedger.Api.Domain;

namespace CardLedger.Api.Tests.Domain;

/// <summary>
/// Dummay POCO "Property" Tests as they skew code coverage metrics otherwise.
/// </summary>
public class CardRowTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var row = new CardRow(cardId, "1234567812345678", 500m, "USD", createdAt);

        // Assert
        Assert.Equal(cardId, row.CardId);
        Assert.Equal("1234567812345678", row.CardNumber);
        Assert.Equal(500m, row.CreditLimit);
        Assert.Equal("USD", row.CurrencyCode);
        Assert.Equal(createdAt, row.CreatedAt);
    }

    [Fact]
    public void Properties_AreMutable()
    {
        // Arrange
        var row = new CardRow(Guid.Empty, string.Empty, 0m, string.Empty, DateTime.UnixEpoch);
        var newId = Guid.NewGuid();

        // Act
        row.CardId = newId;
        row.CardNumber = "9999888877776666";
        row.CreditLimit = 1000m;
        row.CurrencyCode = "AUD";
        row.CreatedAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc);

        // Assert
        Assert.Equal(newId, row.CardId);
        Assert.Equal("9999888877776666", row.CardNumber);
        Assert.Equal(1000m, row.CreditLimit);
        Assert.Equal("AUD", row.CurrencyCode);
        Assert.Equal(new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc), row.CreatedAt);
    }
}
