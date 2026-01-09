using CardLedger.Api.Infrastructure;
using MoneyDataType;
using Moq;

namespace CardLedger.Api.Tests.Infrastructure;

public class CardLedgerRepositoryUnitTests
{
    [Fact]
    public async Task CreateCardAsync_WhenConnectionFails_WrapsException()
    {
        // Arrange
        var factory = new Mock<IDbConnectionFactory>();
        var inner = new InvalidOperationException("boom");
        factory.Setup(f => f.Create()).Returns(new FakeDbConnection(inner));
        var repo = new CardLedgerRepository(factory.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.CreateCardAsync("1234567812345678", Money.Create(10m, "USD"), CancellationToken.None));

        // Assert
        Assert.Contains("CreateCardAsync failed", exception.Message);
        Assert.Equal(inner, exception.InnerException);
    }

    [Fact]
    public async Task GetCardAsync_WhenConnectionFails_WrapsException()
    {
        // Arrange
        var factory = new Mock<IDbConnectionFactory>();
        var inner = new InvalidOperationException("boom");
        factory.Setup(f => f.Create()).Returns(new FakeDbConnection(inner));
        var repo = new CardLedgerRepository(factory.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetCardAsync(Guid.NewGuid(), CancellationToken.None));

        // Assert
        Assert.Contains("GetCardAsync failed", exception.Message);
        Assert.Equal(inner, exception.InnerException);
    }

    [Fact]
    public async Task AppendPurchaseAsync_WhenConnectionFails_WrapsException()
    {
        // Arrange
        var factory = new Mock<IDbConnectionFactory>();
        var inner = new InvalidOperationException("boom");
        factory.Setup(f => f.Create()).Returns(new FakeDbConnection(inner));
        var repo = new CardLedgerRepository(factory.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.AppendPurchaseAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Test",
                DateTimeOffset.UtcNow,
                Money.Create(1m, "USD"),
                Money.Create(1m, "USD"),
                CancellationToken.None));

        // Assert
        Assert.Contains("Postgres Connection Exception", exception.Message);
        Assert.Equal(inner, exception.InnerException);
    }

    [Fact]
    public async Task GetPurchaseAsync_WhenConnectionFails_WrapsException()
    {
        // Arrange
        var factory = new Mock<IDbConnectionFactory>();
        var inner = new InvalidOperationException("boom");
        factory.Setup(f => f.Create()).Returns(new FakeDbConnection(inner));
        var repo = new CardLedgerRepository(factory.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetPurchaseAsync(Guid.NewGuid(), CancellationToken.None));

        // Assert
        Assert.Contains("GetPurchaseAsync failed", exception.Message);
        Assert.Equal(inner, exception.InnerException);
    }

    [Fact]
    public async Task GetTotalSpendAsync_WhenConnectionFails_WrapsException()
    {
        // Arrange
        var factory = new Mock<IDbConnectionFactory>();
        var inner = new InvalidOperationException("boom");
        factory.Setup(f => f.Create()).Returns(new FakeDbConnection(inner));
        var repo = new CardLedgerRepository(factory.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.GetTotalSpendAsync(Guid.NewGuid(), CancellationToken.None));

        // Assert
        Assert.Contains("GetTotalSpendAsync failed", exception.Message);
        Assert.Equal(inner, exception.InnerException);
    }
}
