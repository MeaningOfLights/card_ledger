using System.Globalization;
using CardLedger.Api.Infrastructure;
using MoneyDataType;

namespace CardLedger.Api.Tests.Infrastructure;

public class MoneyFormattingTests
{
    [Fact]
    public void Format_UsesCurrencySymbol_WhenCultureMatches()
    {
        // Arrange
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var money = Money.Create(12.5m, "USD");

            // Act
            var formatted = MoneyFormatting.Format(money);

            // Assert
            Assert.Contains("$", formatted);
            Assert.Contains("12.50", formatted);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Format_UsesCurrencyCode_WhenCultureNotFound()
    {
        // Arrange
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var money = Money.Create(99.99m, "ZZZ");

            // Act
            var formatted = MoneyFormatting.Format(money);

            // Assert
            Assert.Contains("99.99", formatted);
            Assert.EndsWith("ZZZ", formatted, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
