

using System;
using System.Globalization;
using Moq;
using Xunit;

namespace MoneyDataType.Tests;

public class MoneyTests
{
    [Fact]
    public void Constructor_UsesCurrentCulture_WhenNoCurrencyCodeProvided()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            Money money = new(12.34m);

            Assert.Equal("USD", money.CurrencyCode);
            Assert.Equal(12.34m, money.Value);
            Assert.Equal("en-US", money.Culture?.Name);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData(" usd ", "USD")]
    [InlineData("eur", "EUR")]
    public void Constructor_NormalizesCurrencyCode(string input, string expected)
    {
        Money money = new (1.23m, input);

        Assert.Equal(expected, money.CurrencyCode);
    }

    [Fact]
    public void Constructor_WithRegion_UsesRegionCurrency()
    {
        RegionInfo region = new ("AU");
        Money money = new (10m, region);

        Assert.Equal("AUD", money.CurrencyCode);
        Assert.Equal(10m, money.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsWhenCurrencyCodeMissing(string? currencyCode)
    {
        Assert.Throws<MissingCurrencyCodeException>(() => new Money(1m, currencyCode!));
    }

    [Fact]
    public void Equality_UsesValueAndCurrencyCode_CaseInsensitive()
    {
        Money left = new (5m, "usd");
        Money right = new (5m, "USD");
        Money different = new (5.01m, "USD");

        Assert.True(left.Equals(right));
        Assert.True(left == right);
        Assert.False(left != right);
        Assert.False(left.Equals(different));
    }

    [Fact]
    public void HashCode_MatchesForEquivalentValues()
    {
        Money left = new (5m, "usd");
        Money right = new (5m, "USD");

        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Operators_ThrowWhenCurrencyMismatch()
    {
        Money usd = new (10m, "USD");
        Money eur = new (10m, "EUR");

        Assert.Throws<CurrencyMismatchException>(() => _ = usd + eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd - eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd * eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd / eur);
        Assert.Throws<CurrencyMismatchException>(() => _ = usd % eur);
        Assert.Throws<CurrencyMismatchException>(() => Money.Sum([usd, eur]));
    }

    [Fact]
    public void Sum_AddsValuesWithSameCurrency()
    {
        Money a = new (10m, "USD");
        Money b = new (2.5m, "USD");

        Money total = Money.Sum([a, b]);

        Assert.Equal(12.5m, total.Value);
        Assert.Equal("USD", total.CurrencyCode);
    }

    [Fact]
    public void Parse_UsesTrailingCurrencyCode_WhenPresent()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            Money money = Money.Parse("123.45 EUR");

            Assert.Equal(123.45m, money.Value);
            Assert.Equal("EUR", money.CurrencyCode);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void Parse_UsesCurrentCulture_WhenNoCurrencyCode()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            Money money = Money.Parse("$123.45");

            Assert.Equal(123.45m, money.Value);
            Assert.Equal("USD", money.CurrencyCode);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void ToString_UsesCurrencyFormat_WhenCultureMatchesCurrency()
    {
        Money money = new(12.5m, "USD", new CultureInfo("en-US"));

        string formatted = money.ToString();

        Assert.Equal("$12.50", formatted);
    }

    [Fact]
    public void ToString_UsesNumericFormatWithCode_WhenCultureDiffers()
    {
        Money money = new(12.5m, "EUR", new CultureInfo("en-US"));

        string formatted = money.ToString();

        Assert.Equal("12.50 EUR", formatted);
    }

    [Fact]
    public void FormattingRegion_ReturnsRegionFromCulture()
    {
        Money money = new (12.5m, "USD", new CultureInfo("en-US"));

        RegionInfo? region = money.FormattingRegion;

        Assert.NotNull(region);
        Assert.Equal("US", region?.Name);
    }

    [Fact]
    public void ToCurrency_UsesConversionProvider()
    {
        Money money = new (10m, "USD");
        Money converted = new (9m, "EUR");
        Mock<IMoneyConversionProvider> provider = new();
        provider.Setup(p => p.Convert(money, "EUR")).Returns(converted);

        Money result = money.ToCurrency("EUR", provider.Object);

        Assert.Equal(converted, result);
        provider.Verify(p => p.Convert(money, "EUR"), Times.Once);
    }

    [Fact]
    public void WithScale_RoundsToSpecifiedDecimalPlaces()
    {
        Money money = new(1.005m, "USD");

        Money rounded = money.WithScale(2);

        Assert.Equal(1.01m, rounded.Value);
    }

    [Fact]
    public void WithScale_ThrowsForInvalidScale()
    {
        Money money = new(1.23m, "USD");

        Assert.Throws<ArgumentOutOfRangeException>(() => money.WithScale(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => money.WithScale(29));
    }

    [Fact]
    public void ForCardPurchase_RoundsToTwoDecimals()
    {
        Money money = Money.ForCardPurchase(10.005m, "USD", new CultureInfo("en-US"));

        Assert.Equal(10.01m, money.Value);
        Assert.Equal("USD", money.CurrencyCode);
    }
}
