using System.Globalization;

namespace MoneyDataType.Tests;

/// <summary>
/// Dummay POCO "Property" Tests as they skew code coverage metrics otherwise.
/// </summary>
public class MoneyWordFormatterTests
{
    [Fact]
    public void Format_MoneyValue_ReturnsWords()
    {
        // Arrange
        var formatter = new MoneyWordFormatter();
        var money = new Money(12.34m, "USD", new CultureInfo("en-US"));

        // Act
        var result = formatter.Format(null, money, formatter);

        // Assert
        Assert.Contains("Dollar", result);
        Assert.Contains("Cents", result);
    }

    [Fact]
    public void Format_DecimalValue_ReturnsWords()
    {
        // Arrange
        var formatter = new MoneyWordFormatter();
        var value = 0.5m;

        // Act
        var result = formatter.Format(null, value, formatter);

        // Assert
        Assert.Contains("No AUD", result);
        Assert.Contains("Cents", result);
    }

    [Fact]
    public void Format_UnsupportedType_ReturnsEmpty()
    {
        // Arrange
        var formatter = new MoneyWordFormatter();

        // Act
        var result = formatter.Format(null, "not money", formatter);

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
