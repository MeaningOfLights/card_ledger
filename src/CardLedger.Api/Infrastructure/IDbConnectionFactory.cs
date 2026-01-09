using System.Data;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// IDbConnectionFactory interface for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates this instance.
    /// </summary>
    /// <returns></returns>
    IDbConnection Create();
}
