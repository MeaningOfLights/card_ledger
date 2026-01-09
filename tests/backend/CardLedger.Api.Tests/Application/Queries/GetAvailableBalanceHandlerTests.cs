using CardLedger.Api.Application.Queries;
using CardLedger.Api.Domain;
using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using MoneyDataType;
using Moq;

namespace CardLedger.Api.Tests.Application.Queries;

public class GetAvailableBalanceHandlerTests
{
    [Fact]
    public async Task Handle_MissingCard_ThrowsKeyNotFoundException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetAvailableBalanceHandler(repo.Object, fx.Object);
        var cardId = Guid.NewGuid();

        repo.Setup(r => r.GetCardAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetAvailableBalanceQuery(cardId, "USD"), CancellationToken.None));

        // Assert
        Assert.Equal("Card not found.", exception.Message);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_TotalSpendExceedsLimit_ClampsAvailableToZero()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetAvailableBalanceHandler(repo.Object, fx.Object);
        var cardId = Guid.NewGuid();
        var card = new Card
        {
            CardId = cardId,
            CardNumber = "1234567812345678",
            CreditLimit = Money.Create(100m, "USD")
        };

        repo.Setup(r => r.GetCardAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        repo.Setup(r => r.GetTotalSpendAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Money.Create(250m, "USD"));
        fx.Setup(f => f.GetUsdToCurrencyRate("USD", It.IsAny<DateOnly>())).Returns(1m);

        // Act
        var result = await handler.Handle(new GetAvailableBalanceQuery(cardId, "USD"), CancellationToken.None);

        // Assert
        Assert.Equal("USD", result.TargetCurrency);
        Assert.Equal(1m, result.ExchangeRateUsed);
        Assert.Contains("0.00", result.AvailableFormatted);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_TargetCurrencyConversion_UsesFxRate()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetAvailableBalanceHandler(repo.Object, fx.Object);
        var cardId = Guid.NewGuid();
        var card = new Card
        {
            CardId = cardId,
            CardNumber = "1234567812345678",
            CreditLimit = Money.Create(500m, "USD")
        };

        repo.Setup(r => r.GetCardAsync(cardId, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        repo.Setup(r => r.GetTotalSpendAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Money.Create(100m, "USD"));
        fx.Setup(f => f.GetUsdToCurrencyRate("EUR", It.IsAny<DateOnly>())).Returns(0.8m);

        // Act
        var result = await handler.Handle(new GetAvailableBalanceQuery(cardId, "EUR"), CancellationToken.None);

        // Assert
        Assert.Equal("EUR", result.TargetCurrency);
        Assert.Equal(0.8m, result.ExchangeRateUsed);
        Assert.Contains("320,00 €", result.AvailableInTargetCurrencyFormatted);
        repo.VerifyAll();
    }
}
