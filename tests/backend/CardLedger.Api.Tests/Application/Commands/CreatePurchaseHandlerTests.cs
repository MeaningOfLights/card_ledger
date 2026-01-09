using CardLedger.Api.Application.Commands;
using CardLedger.Api.Domain;
using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using MoneyDataType;
using Moq;

namespace CardLedger.Api.Tests.Application.Commands;

public class CreatePurchaseHandlerTests
{
    [Fact]
    public async Task Handle_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new CreatePurchaseHandler(repo.Object, fx.Object);
        var command = new CreatePurchaseCommand
        {
            CardId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid(),
            Description = "",
            TransactionDate = DateTimeOffset.UtcNow,
            Amount = 10m,
            CurrencyCode = "USD"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Description must be 1..50 characters.", exception.Message);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NonPositiveAmount_ThrowsArgumentException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new CreatePurchaseHandler(repo.Object, fx.Object);
        var command = new CreatePurchaseCommand
        {
            CardId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid(),
            Description = "Coffee",
            TransactionDate = DateTimeOffset.UtcNow,
            Amount = 0m,
            CurrencyCode = "USD"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Purchase amount must be positive.", exception.Message);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_NonUsdCurrency_ConvertsToUsdBeforePersisting()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new CreatePurchaseHandler(repo.Object, fx.Object);
        var cardId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid();
        var transactionDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var command = new CreatePurchaseCommand
        {
            CardId = cardId,
            IdempotencyKey = idempotencyKey,
            Description = "Lunch",
            TransactionDate = transactionDate,
            Amount = 10m,
            CurrencyCode = "AUD"
        };

        fx.Setup(f => f.GetUsdToCurrencyRate("AUD", DateOnly.FromDateTime(transactionDate.UtcDateTime)))
            .Returns(2m);

        repo.Setup(r => r.AppendPurchaseAsync(
                cardId,
                idempotencyKey,
                command.Description,
                transactionDate,
                It.Is<Money>(m => m.CurrencyCode == "AUD" && m.Value == 10m),
                It.Is<Money>(m => m.CurrencyCode == "USD" && m.Value == 5m),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LedgerEntry(
                Guid.NewGuid(),
                cardId,
                idempotencyKey,
                command.Description,
                transactionDate,
                Money.Create(10m, "AUD"),
                Money.Create(5m, "USD"),
                "PURCHASE",
                DateTimeOffset.UtcNow));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_MissingFxRate_ThrowsInvalidOperationExceptionWithMessage()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new CreatePurchaseHandler(repo.Object, fx.Object);
        var transactionDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var command = new CreatePurchaseCommand
        {
            CardId = Guid.NewGuid(),
            IdempotencyKey = Guid.NewGuid(),
            Description = "Meal",
            TransactionDate = transactionDate,
            Amount = 12m,
            CurrencyCode = "EUR"
        };

        fx.Setup(f => f.GetUsdToCurrencyRate("EUR", DateOnly.FromDateTime(transactionDate.UtcDateTime)))
            .Throws(new InvalidOperationException("Missing rate"));

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Contains("No FX rate available for EUR", exception.Message);
        repo.VerifyNoOtherCalls();
    }
}
