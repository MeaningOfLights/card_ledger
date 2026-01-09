-- Card Ledger Service schema (idempotent)
-- UUIDv7 values are generated in the API (see Infrastructure/UuidV7.cs)

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS cards (
    card_id            UUID PRIMARY KEY,
    card_number        CHAR(16) NOT NULL UNIQUE,
    credit_limit       NUMERIC(18,2) NOT NULL CHECK (credit_limit > 0),
    currency_code      CHAR(3) NOT NULL DEFAULT 'USD',
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Immutable ledger (append-only). A purchase is represented as a ledger entry with entry_type='PURCHASE'.
CREATE TABLE IF NOT EXISTS ledger_entries (
    ledger_entry_id    UUID PRIMARY KEY,
    card_id            UUID NOT NULL REFERENCES cards(card_id),
    idempotency_key    UUID NOT NULL,
    description        VARCHAR(50) NOT NULL,
    transaction_date   TIMESTAMPTZ NOT NULL,
    amount             NUMERIC(18,2) NOT NULL CHECK (amount > 0),
    original_amount    NUMERIC(18,2) NOT NULL CHECK (original_amount > 0),
    original_currency_code CHAR(3) NOT NULL,
    entry_type         TEXT NOT NULL CHECK (entry_type IN ('PURCHASE', 'REVERSAL')),
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT uq_ledger_idempotency UNIQUE (card_id, idempotency_key)
);

-- Derived state: total spend per card (rebuildable from ledger_entries).
CREATE TABLE IF NOT EXISTS card_spend_projection (
    card_id        UUID PRIMARY KEY REFERENCES cards(card_id),
    total_spend    NUMERIC(18,2) NOT NULL,
    last_entry_at  TIMESTAMPTZ NOT NULL,
    updated_at     TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS ix_ledger_card_date ON ledger_entries(card_id, transaction_date);
CREATE INDEX IF NOT EXISTS ix_ledger_idem ON ledger_entries(card_id, idempotency_key);

-- FX rates loaded from a JSON file in this exercise; table included to show production shape (optional).
CREATE TABLE IF NOT EXISTS fx_rates (
    currency_code   CHAR(3) NOT NULL,
    rate_date       DATE NOT NULL,
    exchange_rate   NUMERIC(18,6) NOT NULL,
    PRIMARY KEY (currency_code, rate_date)
);

-- Outbox (Kafka-ready, not required for this exercise).
CREATE TABLE IF NOT EXISTS outbox_events (
    outbox_event_id  UUID PRIMARY KEY,
    aggregate_type   TEXT NOT NULL,
    aggregate_id     UUID NOT NULL,
    event_type       TEXT NOT NULL,
    payload          JSONB NOT NULL,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
    published_at     TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_outbox_unpublished ON outbox_events(published_at) WHERE published_at IS NULL;
