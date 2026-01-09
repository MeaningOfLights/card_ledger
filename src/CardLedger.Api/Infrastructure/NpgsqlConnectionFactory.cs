using System.Data;
using Npgsql;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// The Npgsql connection factory.
/// </summary>
/// <seealso cref="CardLedger.Api.Infrastructure.IDbConnectionFactory" />
public sealed class NpgsqlConnectionFactory(IConfiguration config) : IDbConnectionFactory
{
    private readonly string _connectionString = config.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Missing ConnectionStrings:Postgres");

    /// <summary>
    /// Creates this instance.
    /// </summary>
    /// <returns></returns>
    public IDbConnection Create()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
