using CardLedger.Api.Infrastructure;
using System.Threading;

namespace CardLedger.Api.Tests.Infrastructure;

public class UuidV7Tests
{
    [Fact]
    public void New_GeneratesVersion7Uuid()
    {
        // Arrange
        var uuid = UuidV7.New();

        // Act
        var bytes = new byte[16];
        uuid.TryWriteBytes(bytes, true, out _);
        var version = (bytes[6] >> 4) & 0x0F;

        // Assert
        Assert.Equal(7, version);
    }

    [Fact]
    public void New_GeneratesRfc4122Variant()
    {
        // Arrange
        var uuid = UuidV7.New();

        // Act
        var bytes = new byte[16];
        uuid.TryWriteBytes(bytes, true, out _);
        var variant = (bytes[8] >> 6) & 0x03;

        // Assert
        Assert.Equal(0b10, variant);
    }

    
}
