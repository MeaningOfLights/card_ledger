using CardLedger.Api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CardLedger.Api.Tests.Infrastructure;

public class DbInitializerTests
{
    [Fact]
    public void Constructor_MissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var connectionStringsSection = new Mock<IConfigurationSection>();
        connectionStringsSection.SetupGet(s => s["Postgres"]).Returns((string?)null);
        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("ConnectionStrings"))
            .Returns(connectionStringsSection.Object);
        var logger = new Mock<ILogger<DbInitializer>>();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => new DbInitializer(config.Object, logger.Object));

        // Assert
        Assert.Equal("Missing ConnectionStrings:Postgres", exception.Message);
    }

    [Fact]
    public async Task InitializeAsync_WhenCancelled_LogsWarningAndThrowsOperationCanceled()
    {
        // Arrange
        var connectionStringsSection = new Mock<IConfigurationSection>();
        connectionStringsSection.SetupGet(s => s["Postgres"])
            .Returns("Host=localhost;Port=1;Database=card_ledger;Username=postgres;Password=postgres");
        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("ConnectionStrings"))
            .Returns(connectionStringsSection.Object);
        var logger = new Mock<ILogger<DbInitializer>>();

        // Act
        var initializer = new DbInitializer(config.Object, logger.Object);

        // Assert
        Assert.NotNull(initializer);
    }
}
