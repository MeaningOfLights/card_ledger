using CardLedger.Api.Services;

namespace CardLedger.Api.Tests.Services;

public class FxRateRowTests
{
    [Fact]
    public void Constructor_AssignsAllProperties()
    {
        // Arrange
        var rateDate = new DateOnly(2026, 1, 5);

        // Act
        var row = new FxRateRow("AUD", rateDate, 1.6m);

        // Assert
        Assert.Equal("AUD", row.Currency);
        Assert.Equal(rateDate, row.RateDate);
        Assert.Equal(1.6m, row.UsdToCurrency);
    }
}
