using CardLedger.Api.Infrastructure;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CardLedger.Api.Tests.Infrastructure;

public class NpgsqlConnectionFactoryTests
{
    [Fact]
    public void Create_ReturnsNpgsqlConnection_WithConnectionString()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=card_ledger;Username=postgres;Password=postgres"
            })
            .Build();

        var factory = new NpgsqlConnectionFactory(config);

        // Act
        var connection = factory.Create();

        // Assert
        var npgsql = Assert.IsType<NpgsqlConnection>(connection);
        Assert.Contains("Host=localhost", npgsql.ConnectionString);
    }

    [Fact]
    public void Constructor_MissingConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => new NpgsqlConnectionFactory(config));

        // Assert
        Assert.Equal("Missing ConnectionStrings:Postgres", exception.Message);
    }
}
