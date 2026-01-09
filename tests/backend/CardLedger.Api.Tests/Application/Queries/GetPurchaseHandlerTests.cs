using CardLedger.Api.Application.Queries;
using CardLedger.Api.Domain;
using CardLedger.Api.Infrastructure;
using CardLedger.Api.Services;
using MoneyDataType;
using Moq;

namespace CardLedger.Api.Tests.Application.Queries;

public class GetPurchaseHandlerTests
{
    [Fact]
    public async Task Handle_PurchaseMissing_ThrowsKeyNotFoundException()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetPurchaseHandler(repo.Object, fx.Object);
        var purchaseId = Guid.NewGuid();

        repo.Setup(r => r.GetPurchaseAsync(purchaseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LedgerEntry?)null);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetPurchaseQuery(purchaseId, "USD"), CancellationToken.None));

        // Assert
        Assert.Equal("Purchase not found.", exception.Message);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_TargetCurrencyMatchesOriginal_ReturnsOriginalAmount()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetPurchaseHandler(repo.Object, fx.Object);
        var purchase = BuildPurchase("AUD", 10m, 6.5m);

        repo.Setup(r => r.GetPurchaseAsync(purchase.LedgerEntryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        // Act
        var result = await handler.Handle(new GetPurchaseQuery(purchase.LedgerEntryId, "AUD"), CancellationToken.None);

        // Assert
        Assert.Equal(purchase.LedgerEntryId, result.PurchaseId);
        Assert.Equal("AUD", result.TargetCurrency);
        Assert.Equal(1m, result.ExchangeRateUsed);
        Assert.Contains("10.00", result.ConvertedAmountFormatted);
        fx.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_TargetCurrencyUsd_UsesUsdAmount()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetPurchaseHandler(repo.Object, fx.Object);
        var purchase = BuildPurchase("AUD", 10m, 5m);

        repo.Setup(r => r.GetPurchaseAsync(purchase.LedgerEntryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);

        // Act
        var result = await handler.Handle(new GetPurchaseQuery(purchase.LedgerEntryId, "USD"), CancellationToken.None);

        // Assert
        Assert.Equal("USD", result.TargetCurrency);
        Assert.Equal(1m, result.ExchangeRateUsed);
        Assert.Contains("5.00", result.ConvertedAmountFormatted);
        fx.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_TargetCurrencyOther_UsesFxRateConversion()
    {
        // Arrange
        var repo = new Mock<ICardLedgerRepository>();
        var fx = new Mock<IFxRateProvider>();
        var handler = new GetPurchaseHandler(repo.Object, fx.Object);
        var purchase = BuildPurchase("USD", 10m, 10m);
        var fxDate = DateOnly.FromDateTime(purchase.TransactionDate.UtcDateTime);

        repo.Setup(r => r.GetPurchaseAsync(purchase.LedgerEntryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchase);
        fx.Setup(f => f.GetUsdToCurrencyRate("EUR", fxDate)).Returns(0.9m);

        // Act
        var result = await handler.Handle(new GetPurchaseQuery(purchase.LedgerEntryId, "EUR"), CancellationToken.None);

        // Assert
        Assert.Equal("EUR", result.TargetCurrency);
        Assert.Equal(0.9m, result.ExchangeRateUsed);
        Assert.Contains("9,00 €", result.ConvertedAmountFormatted);
        fx.VerifyAll();
    }

    private static LedgerEntry BuildPurchase(string originalCurrency, decimal originalValue, decimal usdValue)
    {
        var purchaseId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        return new LedgerEntry(
            purchaseId,
            cardId,
            Guid.NewGuid(),
            "Test",
            new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            Money.Create(originalValue, originalCurrency),
            Money.Create(usdValue, "USD"),
            "PURCHASE",
            DateTimeOffset.UtcNow);
    }
}
