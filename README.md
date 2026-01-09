# Card Ledger Service (Take‑home)

Single-service ASP.NET Core (.NET 8) backend for:
- Creating Cards with a USD credit limit
- Storing Purchase transactions (append-only ledger)
- Retrieving Purchases converted to a target currency
- Retrieving Available Balance converted to a target currency

This repo is intentionally **production-shaped** (CQRS + MediatR, idempotency, immutable ledger, outbox), while keeping runtime simple: **Docker Compose** runs the API and Postgres with one command.

---

## Quick start

From repo root:

Open a Terminal in VsCode, Ctrl + ~

```bash
docker compose up --build

```

- Application: `http://localhost:5173/`
- Swagger: `http://localhost:8080/swagger`
- Health: `http://localhost:8080/health`
- FX rates (JSON): `http://localhost:8080/fx-rates`
- Postgres: `localhost:5432` (db `card_ledger`, user/pass `postgres`)

> The DB schema is created automatically on startup by executing `db/schema.sql`.

---

##  Debugging

Open the sln file in Visual Studio 2022. Set the docker-compose as startup project.

---

## API endpoints

### Create card
`POST /cards`

Body:
```json
{
  "cardNumber": "1234567812345678",
  "creditLimit": 12500.00,
  "currencyCode": "USD"
}
```

### Create purchase (append to ledger)
`POST /cards/{cardId}/purchases`

Headers:
- `Idempotency-Key: <uuid>` (optional; if omitted, the API generates a UUIDv7)

Body:
```json
{
  "description": "Coffee",
  "transactionDate": "2025-12-31",
  "amountUsd": 4.50
}
```

Response:
```json
{ "purchaseId": "...", "idempotencyKey": "..." }
```

### Retrieve purchase converted to currency
`GET /purchases/{purchaseId}?currency=EUR`

Returns:
- original USD amount
- exchange rate used
- converted amount

### Retrieve available balance converted to currency
`GET /cards/{cardId}/available-balance?currency=AUD`

Returns:
- credit limit (USD)
- total spend (USD)
- available (USD)
- exchange rate used
- available in target currency

---

## Money type

The solution includes the provided `Money` library (`src/Shared/Money`) and uses it at the API boundary for:
- Convention over configuration (eg var amountinCentsInUsd)
- Internationalisation
- Converted amounts (rounded to 2dp)

All persisted card/purchase values are stored as `NUMERIC(18,2)` in Postgres to match the requirements (“rounded to the nearest cent”).

---

## FX rate conversion rules

For this take-home we avoid external dependencies and load FX rates from `src/CardLedger.Api/fx_rates.json`.

Rate selection rules match the requirements:

- Find the **latest** rate where:
  - `rateDate <= purchaseDate`
  - `rateDate >= purchaseDate - 6 months`
- If no qualifying rate exists: return an error.

Rounding:
- Converted amounts are rounded to **2 decimal places** (`MidpointRounding.AwayFromZero`).

> Note: Treasury data is typically “foreign per USD”. In this repo the JSON file uses `usdToCurrency` meaning:  
> `converted = usdAmount * usdToCurrency`.

### FX endpoint
`GET /fx-rates` returns the loaded rates.

---

## Ledger correctness & idempotency

### Immutable ledger
Purchases are written to `ledger_entries` with `entry_type='PURCHASE'`. Entries are append-only and never updated.

### Exactly-once application
Idempotency is enforced by a unique constraint:

- `(card_id, idempotency_key)` is unique

The command handler:
- Locks the card row (`SELECT ... FOR UPDATE`) to enforce per-card ordering and prevent credit-limit races
- Inserts the ledger entry with `ON CONFLICT DO NOTHING`
- If the insert conflicts, it returns the previously stored ledger entry (safe retry)

### Derived state (projection)
To avoid scanning ledger entries for every balance read, the write transaction updates a rebuildable projection table:

- `card_spend_projection.total_spend_usd`

Available balance is computed as:

`available = credit_limit - total_spend`

---

## CQRS + MediatR

- Commands:
  - `CreateCardCommand`
  - `CreatePurchaseCommand`
- Queries:
  - `GetPurchaseQuery`
  - `GetAvailableBalanceQuery`

Commands are the only path that mutates state. Queries are read-only.

---

## Outbox pattern (Kafka-ready)

Each successful purchase write creates an Outbox row (`outbox_events`) inside the same database transaction.

> If we needed Kafka, we’d publish `PurchaseCreated` events via outbox → Kafka producer.

For this take-home we do not run a background publisher.

---

## Testing

Result: 84% Coverage  ./tests/backend/CardLedger.Api.Tests/coveragereport/index.html
Priority: Building up a regression testing suite.
Trade-offs: Due to the time limit I dummied coverage for low-risk boilerplate code, (eg DTOs, Config) to not skew results.
Next steps: With more time I woulld expand the coverage with better dB integration tests such as as In-Memory Postgres dB (when it becomes available).

---


## Observability and Monitoring

Due to time constraints this was omitted. I would have implemented Open Telemetry.

---

## Notes for reviewers

- The API is intentionally strict (validation + ProblemDetails responses).
- Postgres schema is idempotent and created on startup.
- UUIDv7 values are generated in the API (see `Infrastructure/UuidV7.cs`).

## TO DO

