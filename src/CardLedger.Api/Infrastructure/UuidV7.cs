using System.Security.Cryptography;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// UUIDv7 generator (time-ordered UUID) implemented per RFC 9562.
/// We use UUIDv7 to create roughly sortable identifiers without DB sequences.
/// </summary>
public static class UuidV7
{
    /// <summary>
    /// Generates a new UUIDv7.
    /// </summary>
    /// <returns></returns>
    public static Guid New()
    {
        Span<byte> b = stackalloc byte[16];
        RandomNumberGenerator.Fill(b);

        // Unix timestamp in milliseconds (48 bits)
        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        b[0] = (byte)((ms >> 40) & 0xFF);
        b[1] = (byte)((ms >> 32) & 0xFF);
        b[2] = (byte)((ms >> 24) & 0xFF);
        b[3] = (byte)((ms >> 16) & 0xFF);
        b[4] = (byte)((ms >> 8) & 0xFF);
        b[5] = (byte)(ms & 0xFF);

        // Version 7 (high nibble of byte 6)
        b[6] = (byte)((b[6] & 0x0F) | 0x70);

        // Variant RFC 4122 (10xx)
        b[8] = (byte)((b[8] & 0x3F) | 0x80);

        return new Guid(b, true);
    }
}
