import DatePicker from 'react-datepicker';
import 'react-datepicker/dist/react-datepicker.css';
import ResultBlock from './components/ResultBlock';
import SectionCard from './components/SectionCard';
import { useCardLedger } from './hooks/useCardLedger';
import styles from './CardLedger.module.scss';

const CardLedger = () => {
  const { state, actions } = useCardLedger();
  const currencies = state.fxRates.currencies.length ? state.fxRates.currencies : ['USD'];

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <h2 className={styles.title}>Card Ledger Application</h2>
        <p className={styles.subtitle}>
          Use these forms to create cards, post purchases, and query balances against the API.
        </p>
      </header>

      <div className={styles.grid}>
        <SectionCard
          title="1. Create Card"
          subtitle="POST /cards"
        >
          <div className={styles.fields}>
            <label className={styles.field}>
              <span className={styles.label}>Card Number</span>
              <input
                className={styles.input}
                inputMode="numeric"
                maxLength={16}
                placeholder="16 digit card number"
                value={state.createCard.cardNumber}
                onChange={(event) => actions.setCreateCardField('cardNumber', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Credit Limit</span>
              <input
                className={styles.input}
                type="number"
                step="0.01"
                placeholder="0.00"
                value={state.createCard.creditLimit}
                onChange={(event) => actions.setCreateCardField('creditLimit', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Currency</span>
              <select className={styles.select} disabled value="USD">
                <option value="USD">USD</option>
              </select>
            </label>
          </div>

          <button className={styles.button} type="button" onClick={actions.submitCreateCard}>
            Create Card
          </button>

          <ResultBlock
            title="Create card response"
            status={state.createCard.status}
            data={state.createCard.response as Record<string, unknown> | undefined}
            error={state.createCard.error}
          />
        </SectionCard>

        <SectionCard
          title="2. Create Purchase"
          subtitle="POST /cards/{cardId}/purchases"
        >
          <div className={styles.fields}>
            <label className={styles.field}>
              <span className={styles.label}>Card ID</span>
              <input
                className={styles.input}
                placeholder="Paste cardId"
                value={state.createPurchase.cardId}
                onChange={(event) => actions.setCreatePurchaseField('cardId', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Description</span>
              <input
                className={styles.input}
                maxLength={50}
                placeholder="A salad roll"
                value={state.createPurchase.description}
                onChange={(event) => actions.setCreatePurchaseField('description', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Transaction Date</span>
              <DatePicker
                className={styles.input}
                selected={state.createPurchase.transactionDate}
                onChange={(date) => actions.setCreatePurchaseDate(date)}
                showTimeSelect
                dateFormat="yyyy-MM-dd HH:mm:ss"
                placeholderText="Select date and time"
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Amount</span>
              <input
                className={styles.input}
                type="number"
                step="0.01"
                placeholder="0.00"
                value={state.createPurchase.amount}
                onChange={(event) => actions.setCreatePurchaseField('amount', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Currency Code</span>
              <select
                className={styles.select}
                value={state.createPurchase.currencyCode}
                onChange={(event) => actions.setCreatePurchaseField('currencyCode', event.target.value)}
              >
                {currencies.map((currency) => (
                  <option key={currency} value={currency}>
                    {currency}
                  </option>
                ))}
              </select>
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Idempotency Key (UUIDv7)</span>
              <input
                className={styles.input}
                readOnly
                value={state.createPurchase.idempotencyKey}
                placeholder="Generated on submit"
              />
            </label>
          </div>

          <button className={styles.button} type="button" onClick={actions.submitCreatePurchase}>
            Create Purchase
          </button>

          <ResultBlock
            title="Create purchase response"
            status={state.createPurchase.status}
            data={state.createPurchase.response as Record<string, unknown> | undefined}
            error={state.createPurchase.error}
          />
        </SectionCard>

        <SectionCard
          title="3. Get Purchase"
          subtitle="GET /purchases/{purchaseId}"
        >
          <div className={styles.fields}>
            <label className={styles.field}>
              <span className={styles.label}>Purchase ID</span>
              <input
                className={styles.input}
                placeholder="Paste purchaseId"
                value={state.getPurchase.purchaseId}
                onChange={(event) => actions.setGetPurchaseField('purchaseId', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Currency</span>
              <select
                className={styles.select}
                value={state.getPurchase.currencyCode}
                onChange={(event) => actions.setGetPurchaseField('currencyCode', event.target.value)}
              >
                {currencies.map((currency) => (
                  <option key={currency} value={currency}>
                    {currency}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <button className={styles.button} type="button" onClick={actions.submitGetPurchase}>
            Get Purchase
          </button>

          <ResultBlock
            title="Purchase details"
            status={state.getPurchase.status}
            data={state.getPurchase.response}
            error={state.getPurchase.error}
          />
        </SectionCard>

        <SectionCard
          title="4. Available Balance"
          subtitle="GET /cards/{cardId}/available-balance"
        >
          <div className={styles.fields}>
            <label className={styles.field}>
              <span className={styles.label}>Card ID</span>
              <input
                className={styles.input}
                placeholder="Paste cardId"
                value={state.availableBalance.cardId}
                onChange={(event) => actions.setAvailableBalanceField('cardId', event.target.value)}
              />
            </label>

            <label className={styles.field}>
              <span className={styles.label}>Currency</span>
              <select
                className={styles.select}
                value={state.availableBalance.currencyCode}
                onChange={(event) => actions.setAvailableBalanceField('currencyCode', event.target.value)}
              >
                {currencies.map((currency) => (
                  <option key={currency} value={currency}>
                    {currency}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <button className={styles.button} type="button" onClick={actions.submitAvailableBalance}>
            Get Available Balance
          </button>

          <ResultBlock
            title="Available balance"
            status={state.availableBalance.status}
            data={state.availableBalance.response}
            error={state.availableBalance.error}
          />
        </SectionCard>
      </div>
    </main>
  );
};

export default CardLedger;
