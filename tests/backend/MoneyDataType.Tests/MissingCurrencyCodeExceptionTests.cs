namespace MoneyDataType.Tests;

/// <summary>
/// Dummay POCO "Property" Tests as they skew code coverage metrics otherwise.
/// </summary>
public class MissingCurrencyCodeExceptionTests
{
    [Fact]
    public void Constructor_DefaultMessage_IsSet()
    {
        // Arrange & Act
        var exception = new MissingCurrencyCodeException();

        // Assert
        Assert.Equal("Currency code is missing.", exception.Message);
    }

    [Fact]
    public void Constructor_CustomMessage_IsSet()
    {
        // Arrange
        var message = "Missing currency!";

        // Act
        var exception = new MissingCurrencyCodeException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }


    [Fact]
    public void Constructor_CustomMessageAndException_IsSet()
    {
        // Arrange
        var message = "Missing currency!";

        // Act
        var exception = new MissingCurrencyCodeException(message, new Exception("Test"));

        // Assert
        Assert.Equal(message, exception.Message);
    }
}
