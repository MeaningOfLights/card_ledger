using CardLedger.Api.Domain;

namespace CardLedger.Api.Tests.Domain;

/// <summary>
/// Dummay POCO "Property" Tests as they skew code coverage metrics otherwise.
/// </summary>
public class LedgerEntryRowTests
{
    [Fact]
    public void Properties_CanBeAssigned()
    {
        // Arrange
        var row = new LedgerEntryRow();
        var entryId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid();
        var transactionDate = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        var createdAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        row.LedgerEntryId = entryId;
        row.CardId = cardId;
        row.IdempotencyKey = idempotencyKey;
        row.Description = "Test purchase";
        row.TransactionDate = transactionDate;
        row.AmountInUsd = 12.34m;
        row.OriginalAmount = 20.5m;
        row.OriginalCurrencyCode = "AUD";
        row.EntryType = "PURCHASE";
        row.CreatedAt = createdAt;

        // Assert
        Assert.Equal(entryId, row.LedgerEntryId);
        Assert.Equal(cardId, row.CardId);
        Assert.Equal(idempotencyKey, row.IdempotencyKey);
        Assert.Equal("Test purchase", row.Description);
        Assert.Equal(transactionDate, row.TransactionDate);
        Assert.Equal(12.34m, row.AmountInUsd);
        Assert.Equal(20.5m, row.OriginalAmount);
        Assert.Equal("AUD", row.OriginalCurrencyCode);
        Assert.Equal("PURCHASE", row.EntryType);
        Assert.Equal(createdAt, row.CreatedAt);
    }
}
