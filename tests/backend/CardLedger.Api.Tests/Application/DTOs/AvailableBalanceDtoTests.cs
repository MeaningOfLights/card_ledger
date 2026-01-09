using CardLedger.Api.Application.DTOs;

namespace CardLedger.Api.Tests.Application.DTOs;

public class AvailableBalanceDtoTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        // Arrange
        var cardId = Guid.NewGuid();

        // Act
        var dto = new AvailableBalanceDto(
            cardId,
            "$1,000.00",
            "$250.00",
            "$750.00",
            "EUR",
            0.92m,
            "690.00 EUR");

        // Assert
        Assert.Equal(cardId, dto.CardId);
        Assert.Equal("$1,000.00", dto.CreditLimitFormatted);
        Assert.Equal("$250.00", dto.TotalSpendFormatted);
        Assert.Equal("$750.00", dto.AvailableFormatted);
        Assert.Equal("EUR", dto.TargetCurrency);
        Assert.Equal(0.92m, dto.ExchangeRateUsed);
        Assert.Equal("690.00 EUR", dto.AvailableInTargetCurrencyFormatted);
    }
}
