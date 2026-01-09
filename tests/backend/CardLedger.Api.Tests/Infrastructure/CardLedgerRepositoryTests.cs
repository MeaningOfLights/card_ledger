// using CardLedger.Api.Infrastructure;
// using Microsoft.Extensions.Configuration;
// using MoneyDataType;
// using Npgsql;

// namespace CardLedger.Api.Tests.Infrastructure;

// public class CardLedgerRepositoryTests : IAsyncLifetime
// {
//    private readonly string _connectionString;
//    private readonly CardLedgerRepository _repository;
//    private readonly List<Guid> _cardIds = new();

//     /// <summary>
//     /// Integration tests with local dB for local testing.
//     /// </summary>
//    public CardLedgerRepositoryTests()
//    {
//        _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
//            ?? "Host=localhost;Port=5432;Database=card_ledger;Username=postgres;Password=postgres";

//        var config = new ConfigurationBuilder()
//            .AddInMemoryCollection(new Dictionary<string, string?>
//            {
//                ["ConnectionStrings:Postgres"] = _connectionString
//            })
//            .Build();

//        var factory = new NpgsqlConnectionFactory(config);
//        _repository = new CardLedgerRepository(factory);
//    }

//    public async Task InitializeAsync()
//    {
//        await using var conn = new NpgsqlConnection(_connectionString);
//        await conn.OpenAsync();
//    }

//    public async Task DisposeAsync()
//    {
//        if (_cardIds.Count == 0)
//        {
//            return;
//        }

//        await using var conn = new NpgsqlConnection(_connectionString);
//        await conn.OpenAsync();

//        foreach (var cardId in _cardIds)
//        {
//            await using var cmd = conn.CreateCommand();
//            cmd.CommandText = @"
// DELETE FROM outbox_events WHERE aggregate_id = @card_id;
// DELETE FROM ledger_entries WHERE card_id = @card_id;
// DELETE FROM card_spend_projection WHERE card_id = @card_id;
// DELETE FROM cards WHERE card_id = @card_id;";
//            cmd.Parameters.AddWithValue("card_id", cardId);
//            await cmd.ExecuteNonQueryAsync();
//        }
//    }

//    [Fact]
//    public async Task CreateCardAsync_PersistsCard()
//    {
//        // Arrange
//        var cardNumber = GenerateCardNumber();
//        var creditLimit = Money.Create(500m, "USD");

//        // Act
//        var card = await _repository.CreateCardAsync(cardNumber, creditLimit, CancellationToken.None);
//        _cardIds.Add(card.CardId);

//        // Assert
//        Assert.Equal(cardNumber, card.CardNumber);
//        Assert.Equal("USD", card.CreditLimit.CurrencyCode);
//        Assert.Equal(500m, card.CreditLimit.Value);
//    }

//    [Fact]
//    public async Task CreateCardAsync_DuplicateCardNumber_ThrowsInvalidOperationException()
//    {
//        // Arrange
//        var cardNumber = GenerateCardNumber();
//        var creditLimit = Money.Create(100m, "USD");

//        // Act
//        var first = await _repository.CreateCardAsync(cardNumber, creditLimit, CancellationToken.None);
//        _cardIds.Add(first.CardId);
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
//            _repository.CreateCardAsync(cardNumber, creditLimit, CancellationToken.None));

//        // Assert
//        Assert.Contains("CreateCardAsync failed", exception.Message);
//        Assert.NotNull(exception.InnerException);
//    }

//    [Fact]
//    public async Task GetCardAsync_MissingCard_ReturnsNull()
//    {
//        // Arrange
//        var missingId = Guid.NewGuid();

//        // Act
//        var card = await _repository.GetCardAsync(missingId, CancellationToken.None);

//        // Assert
//        Assert.Null(card);
//    }

//    [Fact]
//    public async Task GetCardAsync_ExistingCard_ReturnsCard()
//    {
//        // Arrange
//        var cardNumber = GenerateCardNumber();
//        var created = await _repository.CreateCardAsync(cardNumber, Money.Create(200m, "USD"), CancellationToken.None);
//        _cardIds.Add(created.CardId);

//        // Act
//        var fetched = await _repository.GetCardAsync(created.CardId, CancellationToken.None);

//        // Assert
//        Assert.NotNull(fetched);
//        Assert.Equal(created.CardId, fetched!.CardId);
//        Assert.Equal(cardNumber, fetched.CardNumber);
//        Assert.Equal(200m, fetched.CreditLimit.Value);
//    }

//    [Fact]
//    public async Task AppendPurchaseAsync_InsertsLedgerProjectionAndOutbox()
//    {
//        // Arrange
//        var card = await _repository.CreateCardAsync(GenerateCardNumber(), Money.Create(100m, "USD"), CancellationToken.None);
//        _cardIds.Add(card.CardId);
//        var purchaseIdempotency = Guid.NewGuid();
//        var transactionDate = DateTimeOffset.UtcNow;

//        // Act
//        var entry = await _repository.AppendPurchaseAsync(
//            card.CardId,
//            purchaseIdempotency,
//            "Test purchase",
//            transactionDate,
//            Money.Create(20m, "USD"),
//            Money.Create(20m, "USD"),
//            CancellationToken.None);

//        // Assert
//        Assert.Equal(card.CardId, entry.CardId);
//        Assert.Equal(purchaseIdempotency, entry.IdempotencyKey);
//        Assert.Equal("Test purchase", entry.Description);
//        Assert.Equal(20m, entry.Amount.Value);
//        Assert.Equal("USD", entry.Amount.CurrencyCode);

//        await using var conn = new NpgsqlConnection(_connectionString);
//        await conn.OpenAsync();

//        await using var projectionCmd = conn.CreateCommand();
//        projectionCmd.CommandText = "SELECT total_spend FROM card_spend_projection WHERE card_id = @card_id;";
//        projectionCmd.Parameters.AddWithValue("card_id", card.CardId);
//        var totalSpend = (decimal?)await projectionCmd.ExecuteScalarAsync();
//        Assert.Equal(20m, totalSpend);

//        await using var outboxCmd = conn.CreateCommand();
//        outboxCmd.CommandText = "SELECT COUNT(*) FROM outbox_events WHERE aggregate_id = @card_id;";
//        outboxCmd.Parameters.AddWithValue("card_id", card.CardId);
//        var outboxCount = (long)await outboxCmd.ExecuteScalarAsync();
//        Assert.Equal(1L, outboxCount);
//    }

//    [Fact]
//    public async Task AppendPurchaseAsync_MissingCard_ThrowsInvalidOperationException()
//    {
//        // Arrange
//        var missingCardId = Guid.NewGuid();

//        // Act
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AppendPurchaseAsync(
//            missingCardId,
//            Guid.NewGuid(),
//            "No card",
//            DateTimeOffset.UtcNow,
//            Money.Create(5m, "USD"),
//            Money.Create(5m, "USD"),
//            CancellationToken.None));

//        // Assert
//        Assert.Contains("Postgres Connection Exception", exception.Message);
//        Assert.NotNull(exception.InnerException);
//        Assert.Contains("Card not found", exception.InnerException!.Message);
//    }

//    [Fact]
//    public async Task AppendPurchaseAsync_IdempotentKey_ReturnsExistingEntry()
//    {
//        // Arrange
//        var card = await _repository.CreateCardAsync(GenerateCardNumber(), Money.Create(100m, "USD"), CancellationToken.None);
//        _cardIds.Add(card.CardId);
//        var idempotencyKey = Guid.NewGuid();

//        // Act
//        var first = await _repository.AppendPurchaseAsync(
//            card.CardId,
//            idempotencyKey,
//            "Coffee",
//            DateTimeOffset.UtcNow,
//            Money.Create(10m, "USD"),
//            Money.Create(10m, "USD"),
//            CancellationToken.None);

//        var second = await _repository.AppendPurchaseAsync(
//            card.CardId,
//            idempotencyKey,
//            "Coffee",
//            DateTimeOffset.UtcNow,
//            Money.Create(10m, "USD"),
//            Money.Create(10m, "USD"),
//            CancellationToken.None);

//        // Assert
//        Assert.Equal(first.LedgerEntryId, second.LedgerEntryId);

//        var totalSpend = await _repository.GetTotalSpendAsync(card.CardId, CancellationToken.None);
//        Assert.Equal(10m, totalSpend.Value);
//    }

//    [Fact]
//    public async Task GetPurchaseAsync_ExistingPurchase_ReturnsEntry()
//    {
//        // Arrange
//        var card = await _repository.CreateCardAsync(GenerateCardNumber(), Money.Create(100m, "USD"), CancellationToken.None);
//        _cardIds.Add(card.CardId);
//        var idempotencyKey = Guid.NewGuid();
//        var transactionDate = DateTimeOffset.UtcNow;

//        var entry = await _repository.AppendPurchaseAsync(
//            card.CardId,
//            idempotencyKey,
//            "Ticket",
//            transactionDate,
//            Money.Create(15m, "USD"),
//            Money.Create(15m, "USD"),
//            CancellationToken.None);

//        // Act
//        var fetched = await _repository.GetPurchaseAsync(entry.LedgerEntryId, CancellationToken.None);

//        // Assert
//        Assert.NotNull(fetched);
//        Assert.Equal(entry.LedgerEntryId, fetched!.LedgerEntryId);
//        Assert.Equal("Ticket", fetched.Description);
//        Assert.Equal(15m, fetched.Amount.Value);
//    }

//    [Fact]
//    public async Task GetPurchaseAsync_MissingPurchase_ReturnsNull()
//    {
//        // Arrange
//        var missingId = Guid.NewGuid();

//        // Act
//        var result = await _repository.GetPurchaseAsync(missingId, CancellationToken.None);

//        // Assert
//        Assert.Null(result);
//    }

//    [Fact]
//    public async Task AppendPurchaseAsync_ExceedsCreditLimit_ThrowsInvalidOperationException()
//    {
//        // Arrange
//        var card = await _repository.CreateCardAsync(GenerateCardNumber(), Money.Create(5m, "USD"), CancellationToken.None);
//        _cardIds.Add(card.CardId);

//        // Act
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.AppendPurchaseAsync(
//            card.CardId,
//            Guid.NewGuid(),
//            "Large purchase",
//            DateTimeOffset.UtcNow,
//            Money.Create(10m, "USD"),
//            Money.Create(10m, "USD"),
//            CancellationToken.None));

//        // Assert
//        Assert.NotNull(exception.InnerException);
//        Assert.Contains("credit limit", exception.InnerException!.Message);
//    }

//    [Fact]
//    public async Task GetTotalSpendAsync_NoProjection_ReturnsZero()
//    {
//        // Arrange
//        var card = await _repository.CreateCardAsync(GenerateCardNumber(), Money.Create(50m, "USD"), CancellationToken.None);
//        _cardIds.Add(card.CardId);

//        // Act
//        var spend = await _repository.GetTotalSpendAsync(card.CardId, CancellationToken.None);

//        // Assert
//        Assert.Equal(0m, spend.Value);
//        Assert.Equal("USD", spend.CurrencyCode);
//    }

//    [Fact]
//    public async Task GetTotalSpendAsync_WithProjection_ReturnsTotal()
//    {
//        // Arrange
//        var card = await _repository.CreateCardAsync(GenerateCardNumber(), Money.Create(100m, "USD"), CancellationToken.None);
//        _cardIds.Add(card.CardId);

//        await _repository.AppendPurchaseAsync(
//            card.CardId,
//            Guid.NewGuid(),
//            "Train",
//            DateTimeOffset.UtcNow,
//            Money.Create(30m, "USD"),
//            Money.Create(30m, "USD"),
//            CancellationToken.None);

//        // Act
//        var spend = await _repository.GetTotalSpendAsync(card.CardId, CancellationToken.None);

//        // Assert
//        Assert.Equal(30m, spend.Value);
//    }

//    private static string GenerateCardNumber()
//    {
//        var prefix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().PadLeft(16, '0');
//        return prefix.Substring(0, 16);
//    }
// }
