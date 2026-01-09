using Npgsql;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// Ensures the database schema is present on startup.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DbInitializer"/> class.
/// </remarks>
/// <param name="config">The configuration.</param>
/// <param name="logger">The logger.</param>
/// <exception cref="System.InvalidOperationException">Missing ConnectionStrings:Postgres</exception>
public sealed class DbInitializer(IConfiguration config, ILogger<DbInitializer> logger)
{
    private readonly string _connectionString = config.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres");
    private readonly ILogger<DbInitializer> _logger = logger;

    /// <summary>
    /// Initializes the asynchronous initialisation of the database.
    /// </summary>
    /// <param name="ct">The Cancellation Token.</param>
    /// <exception cref="System.Exception">Database initialization failed after multiple attempts.</exception>
    public async Task InitializeAsync(CancellationToken ct)
    {
        // Retry a few times in container startup scenarios.
        var random = new Random();
        TimeSpan maxDelay = TimeSpan.FromSeconds(30);
        int retryCount = 0;
        const int maxAttempts = 5;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(ct);

                var schemaPath = Path.Combine(AppContext.BaseDirectory, "schema.sql");
                var sql = await File.ReadAllTextAsync(schemaPath, ct);

                await using var cmd = new NpgsqlCommand(sql, conn);
                await cmd.ExecuteNonQueryAsync(ct);

                _logger.LogInformation("Database schema ensured.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                _logger.LogWarning(ex, "DB init attempt {Attempt}/{Max} failed. Retrying...", attempt, maxAttempts);

                // Calculate exponential delay: 2^retryCount seconds
                double baseDelaySeconds = Math.Pow(2, retryCount);

                // Add full jitter: random value between 0 and the calculated base delay
                double jitterSeconds = random.NextDouble() * baseDelaySeconds;

                // Ensure the delay doesn't exceed the maximum cap
                TimeSpan delay = TimeSpan.FromSeconds(Math.Min(jitterSeconds, maxDelay.TotalSeconds));

                await Task.Delay(delay, ct);
            }
        }

        throw new Exception("Database initialization failed after multiple attempts.");
    }
}
