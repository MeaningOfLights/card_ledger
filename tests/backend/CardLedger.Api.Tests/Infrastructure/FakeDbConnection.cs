using System.Data;
using System.Data.Common;

namespace CardLedger.Api.Tests.Infrastructure;

internal sealed class FakeDbConnection(Exception exceptionToThrow) : DbConnection
{
    private readonly Exception _exceptionToThrow = exceptionToThrow;

    public override string ConnectionString { get; set; } = string.Empty;
    public override string Database => "fake";
    public override string DataSource => "fake";
    public override string ServerVersion => "0";
    public override ConnectionState State => ConnectionState.Closed;

    public override void ChangeDatabase(string databaseName)
    {
        throw _exceptionToThrow;
    }

    public override void Close()
    {
    }

    public override void Open()
    {
        throw _exceptionToThrow;
    }

    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        return Task.FromException(_exceptionToThrow);
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw _exceptionToThrow;
    }

    protected override DbCommand CreateDbCommand()
    {
        throw _exceptionToThrow;
    }
}
