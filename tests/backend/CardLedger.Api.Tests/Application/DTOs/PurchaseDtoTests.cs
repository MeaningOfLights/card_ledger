using CardLedger.Api.Application.DTOs;

namespace CardLedger.Api.Tests.Application.DTOs;

public class PurchaseDtoTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        // Arrange
        var purchaseId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var transactionDate = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        var dto = new PurchaseDto(
            purchaseId,
            cardId,
            "Lunch",
            transactionDate,
            "$10.00",
            "USD",
            "EUR",
            0.9m,
            "9.00 EUR");

        // Assert
        Assert.Equal(purchaseId, dto.PurchaseId);
        Assert.Equal(cardId, dto.CardId);
        Assert.Equal("Lunch", dto.Description);
        Assert.Equal(transactionDate, dto.TransactionDate);
        Assert.Equal("$10.00", dto.OriginalAmountFormatted);
        Assert.Equal("USD", dto.OriginalCurrency);
        Assert.Equal("EUR", dto.TargetCurrency);
        Assert.Equal(0.9m, dto.ExchangeRateUsed);
        Assert.Equal("9.00 EUR", dto.ConvertedAmountFormatted);
    }
}
