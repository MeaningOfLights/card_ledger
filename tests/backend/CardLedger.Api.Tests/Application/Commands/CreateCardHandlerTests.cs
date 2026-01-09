using CardLedger.Api.Application.Commands;
using CardLedger.Api.Domain;
using CardLedger.Api.Infrastructure;
using MoneyDataType;
using Moq;

namespace CardLedger.Api.Tests.Application.Commands;

public class CreateCardHandlerTests
{
    [Fact]
    public async Task Handle_InvalidCardNumber_ThrowsArgumentException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var handler = new CreateCardHandler(repo.Object);
        var command = new CreateCardCommand
        {
            CardNumber = "1234",
            CreditLimit = 100m,
            CurrencyCode = "USD"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Card number must be a 16-digit numeric string.", exception.Message);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NonUsdCurrency_ThrowsArgumentException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var handler = new CreateCardHandler(repo.Object);
        var command = new CreateCardCommand
        {
            CardNumber = "1234567812345678",
            CreditLimit = 250m,
            CurrencyCode = "EUR"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Credit limit currency must be USD.", exception.Message);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NonPositiveLimit_ThrowsArgumentException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var handler = new CreateCardHandler(repo.Object);
        var command = new CreateCardCommand
        {
            CardNumber = "1234567812345678",
            CreditLimit = 0m,
            CurrencyCode = "USD"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Credit limit must be positive.", exception.Message);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesCardAndFormatsLimit()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var handler = new CreateCardHandler(repo.Object);
        var cardId = Guid.NewGuid();
        var command = new CreateCardCommand
        {
            CardNumber = "1234567812345678",
            CreditLimit = 1234.5m,
            CurrencyCode = "USD"
        };

        repo.Setup(r => r.CreateCardAsync(
                command.CardNumber,
                It.Is<Money>(m => m.Value == 1234.5m && m.CurrencyCode == "USD"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Card
            {
                CardId = cardId,
                CardNumber = command.CardNumber,
                CreditLimit = Money.Create(1234.5m, "USD").WithScale(2)
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(cardId, result.CardId);
        Assert.Equal(command.CardNumber, result.CardNumber);
        Assert.Contains("1,234.50", result.CreditLimitFormatted);
        repo.VerifyAll();
    }
}
