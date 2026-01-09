using System.Globalization;
using MoneyDataType;
using Xunit;

namespace MoneyDataType.Tests;

public class MoneyAdditionalTests
{
    [Fact]
    public void Constructor_Default_UsesCurrentCultureCurrency()
    {
        // Arrange
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            // Act
            var money = new Money(3.21m);

            // Assert
            Assert.Equal("USD", money.CurrencyCode);
            Assert.Equal(3.21m, money.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Constructor_WithRegionInfo_AssignsCurrencyAndCulture()
    {
        // Arrange
        var region = new RegionInfo("AU");

        // Act
        var money = new Money(12.34m, region);

        // Assert
        Assert.Equal("AUD", money.CurrencyCode);
        Assert.Equal(12.34m, money.Value);
        Assert.NotNull(money.Culture);
    }

    [Fact]
    public void Constructor_WithCurrencyAndRegionInfo_AssignsCurrencyAndCulture()
    {
        // Arrange
        var region = new RegionInfo("US");

        // Act
        var money = new Money(99.99m, "USD", region);

        // Assert
        Assert.Equal("USD", money.CurrencyCode);
        Assert.Equal(99.99m, money.Value);
        Assert.NotNull(money.Culture);
    }

    [Fact]
    public void Constructor_WithNullRegion_ThrowsMissingCurrencyCodeException()
    {
        // Arrange
        RegionInfo? region = null;

        // Act
        var exception = Assert.Throws<MissingCurrencyCodeException>(() => new Money(1m, region!));

        // Assert
        Assert.Equal("Region is missing.", exception.Message);
    }

    [Fact]
    public void Create_WithMissingCurrency_ThrowsMissingCurrencyCodeException()
    {
        // Arrange
        var currencyCode = " ";

        // Act
        var exception = Assert.Throws<MissingCurrencyCodeException>(() =>
            Money.Create(5m, currencyCode, null));

        // Assert
        Assert.Equal("Currency code is missing.", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullRegionInCurrencyCtor_ThrowsMissingCurrencyCodeException()
    {
        // Arrange
        RegionInfo? region = null;

        // Act
        var exception = Assert.Throws<MissingCurrencyCodeException>(() => new Money(1m, "USD", region!));

        // Assert
        Assert.Equal("Region is missing.", exception.Message);
    }

    [Fact]
    public void Create_WithRegionString_UsesRegionCurrency()
    {
        // Arrange
        var region = "AU";

        // Act
        var money = Money.Create(10m, "AUD", region);

        // Assert
        Assert.Equal("AUD", money.CurrencyCode);
        Assert.Equal(10m, money.Value);
    }

    [Fact]
    public void FormattingRegion_ReturnsRegionWhenCulturePresent()
    {
        // Arrange
        var money = new Money(5m, "USD", new CultureInfo("en-US"));

        // Act
        var region = money.FormattingRegion;

        // Assert
        Assert.NotNull(region);
        Assert.Equal("US", region!.Name);
    }

    [Fact]
    public void FormattingRegion_ReturnsNullWhenCultureIsInvariant()
    {
        // Arrange
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // Act
            var money = new Money(1m);
            var region = money.FormattingRegion;

            // Assert
            Assert.Equal("US", region!.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Clone_ReturnsEquivalentMoney()
    {
        // Arrange
        var money = new Money(7.5m, "USD", new CultureInfo("en-US"));

        // Act
        var clone = (Money)money.Clone();

        // Assert
        Assert.Equal(money, clone);
    }

    [Fact]
    public void Equals_ObjectOverload_UsesValueAndCurrency()
    {
        // Arrange
        var left = new Money(5m, "USD");
        object right = new Money(5m, "USD");

        // Act
        var equals = left.Equals(right);

        // Assert
        Assert.True(equals);
    }

    [Fact]
    public void GetHashCode_MatchesForEquivalentValues()
    {
        // Arrange
        var left = new Money(5m, "USD");
        var right = new Money(5m, "usd");

        // Act & Assert
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Operators_WithSameCurrency_PerformMath()
    {
        // Arrange
        var a = new Money(10m, "USD");
        var b = new Money(2m, "USD");

        // Act
        var sum = a + b;
        var product = a * b;
        var quotient = a / b;
        var remainder = a % b;

        // Assert
        Assert.Equal(12m, sum.Value);
        Assert.Equal(20m, product.Value);
        Assert.Equal(5m, quotient.Value);
        Assert.Equal(0m, remainder.Value);
    }

    [Fact]
    public void Operators_WithDifferentCurrency_ThrowCurrencyMismatch()
    {
        // Arrange
        var usd = new Money(10m, "USD");
        var eur = new Money(10m, "EUR");

        // Act & Assert
        Assert.Throws<CurrencyMismatchException>(() => _ = usd + eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd * eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd / eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd % eur);
    }

    [Fact]
    public void Operators_CompareByValueAndCurrency()
    {
        // Arrange
        var left = new Money(10m, "USD");
        var right = new Money(10m, "USD");
        var bigger = new Money(12m, "USD");

        // Act & Assert
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.True(bigger >= left);
        Assert.True(left <= bigger);
    }

    [Fact]
    public void Sum_AddsValues()
    {
        // Arrange
        var values = new[] { new Money(1m, "USD"), new Money(2m, "USD"), new Money(3m, "USD") };

        // Act
        var total = Money.Sum(values);

        // Assert
        Assert.Equal(6m, total.Value);
        Assert.Equal("USD", total.CurrencyCode);
    }

    [Fact]
    public void Sum_WithCurrencyMismatch_ThrowsCurrencyMismatchException()
    {
        // Arrange
        var values = new[] { new Money(1m, "USD"), new Money(2m, "EUR") };

        // Act & Assert
        Assert.Throws<CurrencyMismatchException>(() => Money.Sum(values));
    }

    [Fact]
    public void Parse_ThrowsWhenValueIsNull()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Money.Parse(value!));
    }

    [Fact]
    public void Parse_ParsesTrailingCurrencyCode()
    {
        // Arrange
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            // Act
            var money = Money.Parse("123.45 AUD");

            // Assert
            Assert.Equal("AUD", money.CurrencyCode);
            Assert.Equal(123.45m, money.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Parse_UsesCurrentCultureWhenNoCurrencyCode()
    {
        // Arrange
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            // Act
            var money = Money.Parse("$45.60");

            // Assert
            Assert.Equal("USD", money.CurrencyCode);
            Assert.Equal(45.60m, money.Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
