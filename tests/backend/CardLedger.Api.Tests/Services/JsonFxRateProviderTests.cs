using CardLedger.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace CardLedger.Api.Tests.Services;

public class JsonFxRateProviderTests
{
    [Fact]
    public void GetAll_ReturnsLoadedRows()
    {
        // Arrange
        var env = new Mock<IWebHostEnvironment>();
        var tempDir = CreateTempDirectory();
        var jsonPath = Path.Combine(tempDir, "fx_rates.json");
        File.WriteAllText(jsonPath, "[{\"currency\":\"AUD\",\"rateDate\":\"2026-01-01\",\"usdToCurrency\":1.5}]");
        env.Setup(e => e.ContentRootPath).Returns(tempDir);

        // Act
        var provider = new JsonFxRateProvider(env.Object);
        var rows = provider.GetAll();

        // Assert
        Assert.Single(rows);
        Assert.Equal("AUD", rows[0].Currency);
        Assert.Equal(new DateOnly(2026, 1, 1), rows[0].RateDate);
        Assert.Equal(1.5m, rows[0].UsdToCurrency);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void GetUsdToCurrencyRate_ReturnsOneForUsd()
    {
        // Arrange
        var env = new Mock<IWebHostEnvironment>();
        var tempDir = CreateTempDirectory();
        var jsonPath = Path.Combine(tempDir, "fx_rates.json");
        File.WriteAllText(jsonPath, "[]");
        env.Setup(e => e.ContentRootPath).Returns(tempDir);
        var provider = new JsonFxRateProvider(env.Object);

        // Act
        var rate = provider.GetUsdToCurrencyRate("USD", new DateOnly(2026, 1, 1));

        // Assert
        Assert.Equal(1m, rate);

        Directory.Delete(tempDir, true);
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fxrates-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
