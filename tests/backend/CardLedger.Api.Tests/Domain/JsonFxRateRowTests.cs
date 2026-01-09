using CardLedger.Api.Domain;

namespace CardLedger.Api.Tests.Domain;

/// <summary>
/// Dummay POCO "Property" Tests as they skew code coverage metrics otherwise.
/// </summary>
public class JsonFxRateRowTests
{
    [Fact]
    public void Properties_DefaultToNullOrZero()
    {
        // Arrange
        var row = new JsonFxRateRow();

        // Act
        var currency = row.Currency;
        var rateDate = row.RateDate;
        var usdToCurrency = row.UsdToCurrency;

        // Assert
        Assert.Null(currency);
        Assert.Null(rateDate);
        Assert.Equal(0m, usdToCurrency);
    }

    [Fact]
    public void Properties_CanBeAssigned()
    {
        // Arrange
        JsonFxRateRow row = new()
        {
            // Act
            Currency = "USD",
            RateDate = "2026-01-01",
            UsdToCurrency = 1.25m
        };

        // Assert
        Assert.Equal("USD", row.Currency);
        Assert.Equal("2026-01-01", row.RateDate);
        Assert.Equal(1.25m, row.UsdToCurrency);
    }
}
