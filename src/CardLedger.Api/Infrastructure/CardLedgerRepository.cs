using CardLedger.Api.Domain;
using System.Data;
using System.Data.Common;
using System.Transactions;
using Dapper;
using MoneyDataType;
using Npgsql;

namespace CardLedger.Api.Infrastructure;

/// <summary>
/// Dapper-backed repository for cards and ledger entries.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CardLedgerRepository"/> class.
/// </remarks>
/// <param name="factory">The database connection factory.</param>
public sealed class CardLedgerRepository(IDbConnectionFactory factory) : ICardLedgerRepository
{
    private readonly IDbConnectionFactory _factory = factory;

    private IDbConnection Open() => _factory.Create();

    /// <summary>
    /// Creates a card asynchronously.
    /// </summary>
    /// <param name="cardNumber">The card number.</param>
    /// <param name="creditLimit">The credit limit.</param>
    /// <param name="ct">The Cancellation Token.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">CreateCardAsync failed. cardNumber={cardNumber}</exception>
    public async Task<Card> CreateCardAsync(string cardNumber, Money creditLimit, CancellationToken ct)
    {
        try
        {
            var cardId = UuidV7.New();
            var creditLimitScaled = creditLimit.WithScale(2);
            using var conn = (DbConnection)Open();
            await conn.OpenAsync(ct);

            const string sql = @"
            INSERT INTO cards (card_id, card_number, credit_limit, currency_code)
            VALUES (@card_id, @card_number, @credit_limit, @currency_code)
            RETURNING card_id AS CardId,
                        card_number AS CardNumber,
                        credit_limit AS CreditLimit,
                        currency_code AS CurrencyCode,
                        created_at AS CreatedAt;";

            var row = await conn.QuerySingleAsync<CardRow>(new CommandDefinition(sql, new
            {
                card_id = cardId,
                card_number = cardNumber,
                credit_limit = creditLimitScaled.Value,
                currency_code = creditLimitScaled.CurrencyCode
            }, cancellationToken: ct));
            return MapCard(row);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"CreateCardAsync failed. cardNumber={cardNumber}",ex);
        }
    }

    /// <summary>
    /// Gets a card asynchronously.
    /// </summary>
    /// <param name="cardId">The card identifier.</param>
    /// <param name="ct">The Cancellation Token.</param>
    /// <returns></returns>
    /// <exception cref="System.InvalidOperationException">GetCardAsync failed. cardId={cardId}</exception>
    public async Task<Card?> GetCardAsync(Guid cardId, CancellationToken ct)
    {
        try
        {
            using var conn = (DbConnection)Open();
            await conn.OpenAsync(ct);

            const string sql = @"
            SELECT card_id AS CardId,
                   card_number AS CardNumber,
                   credit_limit AS CreditLimit,
                   currency_code AS CurrencyCode,
                   created_at AS CreatedAt
            FROM cards
            WHERE card_id = @card_id;";

            var row = await conn.QuerySingleOrDefaultAsync<CardRow>(new CommandDefinition(sql, new { card_id = cardId }, cancellationToken: ct));
            return row is null ? null : MapCard(row);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"GetCardAsync failed. cardId={cardId}", ex);
        }
    }

    /// <summary>
    /// Appends a purchase to the ledger for the given card.
    /// </summary>
    /// <remarks>
    /// Uses pessimistic locking to ensure sequential processing per card.
    /// Ensures Idempotency by (cardId, idempotencyKey).
    /// Updates the spend projection transactionally.
    /// Creates an outbox_events entry for further processing.
    /// </remarks>
    /// <param name="cardId">The Card Id.</param>
    /// <param name="idempotencyKey">The idempotencyKey.</param>
    /// <param name="description">The description.</param>
    /// <param name="transactionDate">The transactionDate.</param>
    /// <param name="originalAmount">The originalAmount.</param>
    /// <param name="amountInUsd">The amountInUsd.</param>
    /// <param name="ct">The CancellationToken.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<LedgerEntry> AppendPurchaseAsync(Guid cardId, Guid idempotencyKey, string description, DateTimeOffset transactionDate, Money originalAmount, Money amountInUsd, CancellationToken ct)
    {
        try
        {
            using var conn = (DbConnection)Open();
            await conn.OpenAsync(ct);
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Acquire a pessimistic row-level lock on the specific card to ensure 
            // sequential transaction processing and prevent race conditions.
            const string lockSql = @"SELECT card_id FROM cards WHERE card_id = @card_id FOR UPDATE;";
            var locked = await conn.ExecuteScalarAsync<Guid?>(new CommandDefinition(lockSql, new { card_id = cardId }, cancellationToken: ct))
                ?? throw new KeyNotFoundException("Card not found.");

            // Read current totals inside the same transaction.
            const string totalsSql = @"
                    SELECT credit_limit AS CreditLimit
                    FROM cards WHERE card_id = @card_id;

                    SELECT COALESCE(total_spend, 0) AS TotalSpend
                    FROM card_spend_projection WHERE card_id = @card_id;";
            using var multi = await conn.QueryMultipleAsync(new CommandDefinition(totalsSql, new { card_id = cardId }, cancellationToken: ct));
            var creditLimit = await multi.ReadSingleAsync<decimal>();
            var totalSpend = (await multi.ReadSingleOrDefaultAsync<decimal?>()) ?? 0m;

            var amountInUsaScaled = amountInUsd.WithScale(2);
            var originalAmountScaled = originalAmount.WithScale(2);
            var newTotal = totalSpend + amountInUsaScaled.Value;
            if (newTotal > creditLimit)
            {
                throw new InvalidOperationException("Purchase would exceed credit limit.");
            }

            // Insert append-only ledger entry (exactly-once by unique constraint).
            var purchaseId = UuidV7.New();

            const string insertSql = @"
                    INSERT INTO ledger_entries
                    (ledger_entry_id, card_id, idempotency_key, description, transaction_date, amount, original_amount, original_currency_code, entry_type)
                    VALUES
                    (@ledger_entry_id, @card_id, @idempotency_key, @description, @transaction_date, @amount, @original_amount, @original_currency_code, 'PURCHASE')
                    ON CONFLICT (card_id, idempotency_key) DO NOTHING
                    RETURNING ledger_entry_id AS LedgerEntryId,
                              card_id AS CardId,
                              idempotency_key AS IdempotencyKey,
                              description AS Description,
                              transaction_date AS TransactionDate,
                              amount AS AmountInUsd,
                              original_amount AS OriginalAmount,
                              original_currency_code AS OriginalCurrencyCode,
                              entry_type AS EntryType,
                              created_at AS CreatedAt;";

            var insertedRow = await conn.QuerySingleOrDefaultAsync<LedgerEntryRow>(new CommandDefinition(insertSql, new
            {
                ledger_entry_id = purchaseId,
                card_id = cardId,
                idempotency_key = idempotencyKey,
                description,
                transaction_date = transactionDate,
                amount = amountInUsaScaled.Value,
                original_amount = originalAmountScaled.Value,
                original_currency_code = originalAmountScaled.CurrencyCode
            }, cancellationToken: ct));

            if (insertedRow is null)
            {
                // Idempotent replay: fetch the existing entry.
                const string existingSql = @"
                        SELECT ledger_entry_id AS LedgerEntryId,
                               card_id AS CardId,
                               idempotency_key AS IdempotencyKey,
                               description AS Description,
                               transaction_date AS TransactionDate,
                               amount AS AmountInUsd,
                               original_amount AS OriginalAmount,
                               original_currency_code AS OriginalCurrencyCode,
                               entry_type AS EntryType,
                               created_at AS CreatedAt
                        FROM ledger_entries
                        WHERE card_id = @card_id AND idempotency_key = @idempotency_key;";
                insertedRow = await conn.QuerySingleAsync<LedgerEntryRow>(new CommandDefinition(existingSql, new { card_id = cardId, idempotency_key = idempotencyKey }, cancellationToken: ct));
                scope.Complete();
                return MapLedgerEntry(insertedRow);
            }

            // Update spend projection.
            const string upsertProjection = @"
                    INSERT INTO card_spend_projection (card_id, total_spend, last_entry_at)
                    VALUES (@card_id, @total_spend, @last_entry_at)
                    ON CONFLICT (card_id) DO UPDATE
                    SET total_spend = EXCLUDED.total_spend,
                        last_entry_at = EXCLUDED.last_entry_at,
                        updated_at = now();";

            await conn.ExecuteAsync(new CommandDefinition(upsertProjection, new
            {
                card_id = cardId,
                total_spend = newTotal,
                last_entry_at = DateTimeOffset.UtcNow
            }, cancellationToken: ct));

            // Outbox event
            const string outboxSql = @"
                    INSERT INTO outbox_events (outbox_event_id, aggregate_type, aggregate_id, event_type, payload)
                    VALUES (@id, 'Card', @aggregate_id, 'PurchaseCreated', @payload::jsonb);";

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                purchaseId = insertedRow.LedgerEntryId,
                cardId = insertedRow.CardId,
                amount = insertedRow.AmountInUsd,
                transactionDate = insertedRow.TransactionDate
            });
            await conn.ExecuteAsync(new CommandDefinition(outboxSql, new
            {
                id = UuidV7.New(),
                aggregate_id = cardId,
                payload
            }, cancellationToken: ct));

            scope.Complete();
            return MapLedgerEntry(insertedRow);
        }
        catch (PostgresException ex)
        {
            throw new InvalidOperationException($"AppendPurchaseAsync failed with cardId={cardId}. Postgres {ex.SqlState}: {ex.MessageText}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Postgres Connection Exception", ex);
        }
    }

    /// <summary>
    /// Gets a purchase by ID.
    /// </summary>
    /// <param name="purchaseId">The purchaseId.</param>
    /// <param name="ct">The CancellationToken.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<LedgerEntry?> GetPurchaseAsync(Guid purchaseId, CancellationToken ct)
    {
        try
        {
            using var conn = (DbConnection)Open();
            await conn.OpenAsync(ct);

            const string sql = @"
                SELECT ledger_entry_id AS LedgerEntryId,
                    card_id AS CardId,
                    idempotency_key AS IdempotencyKey,
                    description AS Description,
                    transaction_date AS TransactionDate,
                    amount AS AmountInUsd,
                    original_amount AS OriginalAmount,
                    original_currency_code AS OriginalCurrencyCode,
                    entry_type AS EntryType,
                    created_at AS CreatedAt
                FROM ledger_entries
                WHERE ledger_entry_id = @id AND entry_type = 'PURCHASE';";

            var row = await conn.QuerySingleOrDefaultAsync<LedgerEntryRow>(new CommandDefinition(sql, new { id = purchaseId }, cancellationToken: ct));
            return row is null ? null : MapLedgerEntry(row);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"GetPurchaseAsync failed. purchaseId={purchaseId}", ex);
        }
    }

    /// <summary>
    /// Gets the total spend for a card.
    /// </summary>
    /// <param name="cardId">The Card Id.</param>
    /// <param name="ct">The CancellationToken.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Money> GetTotalSpendAsync(Guid cardId, CancellationToken ct)
    {
        try
        {
            using var conn = (DbConnection)Open();
            await conn.OpenAsync(ct);

            const string sql = @"SELECT COALESCE(total_spend, 0) FROM card_spend_projection WHERE card_id = @card_id;";
            var totalSpend = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(sql, new { card_id = cardId }, cancellationToken: ct));
            return new Money(totalSpend, "USD").WithScale(2);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"GetTotalSpendAsync failed. cardId={cardId}", ex);
        }
    }

    /// <summary>
    /// Maps the card.
    /// </summary>
    /// <param name="row">The Card Row.</param>
    /// <returns></returns>
    private static Card MapCard(CardRow row)
    {
        var creditLimit = new Money(row.CreditLimit, row.CurrencyCode).WithScale(2);
        var createdAt = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc);
        return new Card { CardId = row.CardId, CardNumber = row.CardNumber, CreditLimit = creditLimit, CreatedAt = new DateTimeOffset(createdAt) };
    }

    /// <summary>
    /// Maps the ledger entry.
    /// </summary>
    /// <param name="row">The Ledger Entry Row.</param>
    /// <returns></returns>
    private static LedgerEntry MapLedgerEntry(LedgerEntryRow row)
    {
        var amount = new Money(row.AmountInUsd, "USD").WithScale(2);
        var originalAmount = new Money(row.OriginalAmount, row.OriginalCurrencyCode).WithScale(2);
        var createdAt = DateTime.SpecifyKind(row.CreatedAt, DateTimeKind.Utc);
        return new LedgerEntry(row.LedgerEntryId, row.CardId, row.IdempotencyKey, row.Description, row.TransactionDate, originalAmount, amount, row.EntryType, new DateTimeOffset(createdAt));
    }
}

