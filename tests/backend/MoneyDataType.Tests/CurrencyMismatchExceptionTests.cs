namespace MoneyDataType.Tests;

/// <summary>
/// Dummay POCO "Property" Tests as they skew code coverage metrics otherwise.
/// </summary>
public class CurrencyMismatchExceptionTests
{
    [Fact]
    public void Constructor_DefaultMessage_IsSet()
    {
        // Arrange & Act
        var exception = new CurrencyMismatchException();

        // Assert
        Assert.Equal("Currencies don't match in supplied parameters.", exception.Message);
    }

    [Fact]
    public void Constructor_CustomMessage_IsSet()
    {
        // Arrange
        var message = "Custom mismatch message.";

        // Act
        var exception = new CurrencyMismatchException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_CustomMessageAndException_IsSet()
    {
        // Arrange
        var message = "Custom mismatch message.";

        // Act
        var exception = new CurrencyMismatchException(message, new Exception("Test"));

        // Assert
        Assert.Equal(message, exception.Message);
    }
}