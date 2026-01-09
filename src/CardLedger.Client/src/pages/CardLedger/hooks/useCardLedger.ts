import { useCallback, useEffect, useReducer } from 'react';
import {
  createCard,
  createPurchase,
  fetchFxRates,
  getAvailableBalance,
  getPurchase,
} from '../actions/cardLedgerActions';
import {
  CardLedgerState,
  cardLedgerReducer,
  initialCardLedgerState,
} from '../reducer/cardLedgerReducer';
import { CreateCardPayload, CreatePurchasePayload } from '../types';
import { uuidv7 } from '../utils/uuidv7';

const extractCurrencies = (rows: { currency?: string; Currency?: string }[]): string[] => {
  const set = new Set<string>();
  rows.forEach((row) => {
    const code = row.currency || row.Currency;
    if (code) {
      set.add(code);
    }
  });

  return Array.from(set).sort();
};

export const useCardLedger = () => {
  const [state, dispatch] = useReducer(cardLedgerReducer, initialCardLedgerState);

  useEffect(() => {
    let active = true;

    const loadFxRates = async () => {
      dispatch({ type: 'fxRates/loading' });
      try {
        const rows = await fetchFxRates();
        if (!active) {
          return;
        }
        dispatch({ type: 'fxRates/success', payload: extractCurrencies(rows) });
      } catch (error) {
        if (!active) {
          return;
        }
        const message = error instanceof Error ? error.message : 'Unable to load FX rates.';
        dispatch({ type: 'fxRates/error', payload: message });
      }
    };

    void loadFxRates();

    return () => {
      active = false;
    };
  }, []);

  const setCreateCardField = useCallback((field: 'cardNumber' | 'creditLimit', value: string) => {
    dispatch({ type: 'createCard/field', field, value });
  }, []);

  const setCreatePurchaseField = useCallback(
    (field: 'cardId' | 'description' | 'amount' | 'currencyCode' | 'idempotencyKey', value: string) => {
      dispatch({ type: 'createPurchase/field', field, value });
    },
    [],
  );

  const setCreatePurchaseDate = useCallback((value: Date | null) => {
    dispatch({ type: 'createPurchase/date', value });
  }, []);

  const setGetPurchaseField = useCallback((field: 'purchaseId' | 'currencyCode', value: string) => {
    dispatch({ type: 'getPurchase/field', field, value });
  }, []);

  const setAvailableBalanceField = useCallback(
    (field: 'cardId' | 'currencyCode', value: string) => {
      dispatch({ type: 'availableBalance/field', field, value });
    },
    [],
  );

  const submitCreateCard = useCallback(async () => {
    dispatch({ type: 'createCard/request' });
    const payload: CreateCardPayload = {
      cardNumber: state.createCard.cardNumber.trim(),
      creditLimit: Number(state.createCard.creditLimit),
      currencyCode: state.createCard.currencyCode,
    };

    try {
      const response = await createCard(payload);
      dispatch({ type: 'createCard/success', payload: response });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to create card.';
      dispatch({ type: 'createCard/error', payload: message });
    }
  }, [state.createCard.cardNumber, state.createCard.creditLimit, state.createCard.currencyCode]);

  const submitCreatePurchase = useCallback(async () => {
    dispatch({ type: 'createPurchase/request' });
    const idempotencyKey = uuidv7();
    dispatch({ type: 'createPurchase/field', field: 'idempotencyKey', value: idempotencyKey });

    const payload: CreatePurchasePayload = {
      description: state.createPurchase.description.trim(),
      transactionDate: state.createPurchase.transactionDate
        ? state.createPurchase.transactionDate.toISOString()
        : new Date().toISOString(),
      amount: Number(state.createPurchase.amount),
      currencyCode: state.createPurchase.currencyCode,
    };

    try {
      const response = await createPurchase(
        state.createPurchase.cardId.trim(),
        payload,
        idempotencyKey,
      );
      dispatch({ type: 'createPurchase/success', payload: response });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to create purchase.';
      dispatch({ type: 'createPurchase/error', payload: message });
    }
  }, [
    state.createPurchase.amount,
    state.createPurchase.cardId,
    state.createPurchase.currencyCode,
    state.createPurchase.description,
    state.createPurchase.transactionDate,
  ]);

  const submitGetPurchase = useCallback(async () => {
    dispatch({ type: 'getPurchase/request' });
    try {
      const response = await getPurchase(
        state.getPurchase.purchaseId.trim(),
        state.getPurchase.currencyCode,
      );
      dispatch({ type: 'getPurchase/success', payload: response });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to load purchase.';
      dispatch({ type: 'getPurchase/error', payload: message });
    }
  }, [state.getPurchase.currencyCode, state.getPurchase.purchaseId]);

  const submitAvailableBalance = useCallback(async () => {
    dispatch({ type: 'availableBalance/request' });
    try {
      const response = await getAvailableBalance(
        state.availableBalance.cardId.trim(),
        state.availableBalance.currencyCode,
      );
      dispatch({ type: 'availableBalance/success', payload: response });
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Unable to load available balance.';
      dispatch({ type: 'availableBalance/error', payload: message });
    }
  }, [state.availableBalance.cardId, state.availableBalance.currencyCode]);

  return {
    state,
    actions: {
      setCreateCardField,
      setCreatePurchaseField,
      setCreatePurchaseDate,
      setGetPurchaseField,
      setAvailableBalanceField,
      submitCreateCard,
      submitCreatePurchase,
      submitGetPurchase,
      submitAvailableBalance,
    },
  };
};
