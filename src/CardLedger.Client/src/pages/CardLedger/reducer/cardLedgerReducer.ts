import { ApiResult, CreateCardResponse, CreatePurchaseResponse, RequestStatus } from '../types';

type FxRatesState = {
  status: RequestStatus;
  currencies: string[];
  error?: string;
};

type CreateCardState = {
  cardNumber: string;
  creditLimit: string;
  currencyCode: string;
  status: RequestStatus;
  response?: CreateCardResponse;
  error?: string;
};

type CreatePurchaseState = {
  cardId: string;
  description: string;
  transactionDate: Date | null;
  amount: string;
  currencyCode: string;
  idempotencyKey: string;
  status: RequestStatus;
  response?: CreatePurchaseResponse;
  error?: string;
};

type GetPurchaseState = {
  purchaseId: string;
  currencyCode: string;
  status: RequestStatus;
  response?: ApiResult;
  error?: string;
};

type AvailableBalanceState = {
  cardId: string;
  currencyCode: string;
  status: RequestStatus;
  response?: ApiResult;
  error?: string;
};

export type CardLedgerState = {
  fxRates: FxRatesState;
  createCard: CreateCardState;
  createPurchase: CreatePurchaseState;
  getPurchase: GetPurchaseState;
  availableBalance: AvailableBalanceState;
};

export type CardLedgerAction =
  | { type: 'fxRates/loading' }
  | { type: 'fxRates/success'; payload: string[] }
  | { type: 'fxRates/error'; payload: string }
  | { type: 'createCard/field'; field: 'cardNumber' | 'creditLimit'; value: string }
  | { type: 'createCard/request' }
  | { type: 'createCard/success'; payload: CreateCardResponse }
  | { type: 'createCard/error'; payload: string }
  | {
      type: 'createPurchase/field';
      field: 'cardId' | 'description' | 'amount' | 'currencyCode' | 'idempotencyKey';
      value: string;
    }
  | { type: 'createPurchase/date'; value: Date | null }
  | { type: 'createPurchase/request' }
  | { type: 'createPurchase/success'; payload: CreatePurchaseResponse }
  | { type: 'createPurchase/error'; payload: string }
  | { type: 'getPurchase/field'; field: 'purchaseId' | 'currencyCode'; value: string }
  | { type: 'getPurchase/request' }
  | { type: 'getPurchase/success'; payload: ApiResult }
  | { type: 'getPurchase/error'; payload: string }
  | { type: 'availableBalance/field'; field: 'cardId' | 'currencyCode'; value: string }
  | { type: 'availableBalance/request' }
  | { type: 'availableBalance/success'; payload: ApiResult }
  | { type: 'availableBalance/error'; payload: string };

export const initialCardLedgerState: CardLedgerState = {
  fxRates: {
    status: 'idle',
    currencies: ['USD'],
  },
  createCard: {
    cardNumber: '',
    creditLimit: '',
    currencyCode: 'USD',
    status: 'idle',
  },
  createPurchase: {
    cardId: '',
    description: '',
    transactionDate: null,
    amount: '',
    currencyCode: 'USD',
    idempotencyKey: '',
    status: 'idle',
  },
  getPurchase: {
    purchaseId: '',
    currencyCode: 'USD',
    status: 'idle',
  },
  availableBalance: {
    cardId: '',
    currencyCode: 'USD',
    status: 'idle',
  },
};

export const cardLedgerReducer = (
  state: CardLedgerState,
  action: CardLedgerAction,
): CardLedgerState => {
  switch (action.type) {
    case 'fxRates/loading':
      return { ...state, fxRates: { ...state.fxRates, status: 'loading', error: undefined } };
    case 'fxRates/success':
      return {
        ...state,
        fxRates: {
          status: 'success',
          currencies: action.payload.length ? action.payload : ['USD'],
        },
      };
    case 'fxRates/error':
      return { ...state, fxRates: { ...state.fxRates, status: 'error', error: action.payload } };
    case 'createCard/field':
      return {
        ...state,
        createCard: { ...state.createCard, [action.field]: action.value },
      };
    case 'createCard/request':
      return {
        ...state,
        createCard: { ...state.createCard, status: 'loading', error: undefined },
      };
    case 'createCard/success':
      return {
        ...state,
        createCard: {
          ...state.createCard,
          status: 'success',
          response: action.payload,
          error: undefined,
        },
        createPurchase: { ...state.createPurchase, cardId: action.payload.cardId },
        availableBalance: { ...state.availableBalance, cardId: action.payload.cardId },
      };
    case 'createCard/error':
      return {
        ...state,
        createCard: { ...state.createCard, status: 'error', error: action.payload },
      };
    case 'createPurchase/field':
      return {
        ...state,
        createPurchase: { ...state.createPurchase, [action.field]: action.value },
      };
    case 'createPurchase/date':
      return {
        ...state,
        createPurchase: { ...state.createPurchase, transactionDate: action.value },
      };
    case 'createPurchase/request':
      return {
        ...state,
        createPurchase: { ...state.createPurchase, status: 'loading', error: undefined },
      };
    case 'createPurchase/success':
      return {
        ...state,
        createPurchase: {
          ...state.createPurchase,
          status: 'success',
          response: action.payload,
          error: undefined,
        },
        getPurchase: { ...state.getPurchase, purchaseId: action.payload.purchaseId },
      };
    case 'createPurchase/error':
      return {
        ...state,
        createPurchase: { ...state.createPurchase, status: 'error', error: action.payload },
      };
    case 'getPurchase/field':
      return { ...state, getPurchase: { ...state.getPurchase, [action.field]: action.value } };
    case 'getPurchase/request':
      return { ...state, getPurchase: { ...state.getPurchase, status: 'loading', error: undefined } };
    case 'getPurchase/success':
      return {
        ...state,
        getPurchase: { ...state.getPurchase, status: 'success', response: action.payload },
      };
    case 'getPurchase/error':
      return {
        ...state,
        getPurchase: { ...state.getPurchase, status: 'error', error: action.payload },
      };
    case 'availableBalance/field':
      return {
        ...state,
        availableBalance: { ...state.availableBalance, [action.field]: action.value },
      };
    case 'availableBalance/request':
      return {
        ...state,
        availableBalance: {
          ...state.availableBalance,
          status: 'loading',
          error: undefined,
        },
      };
    case 'availableBalance/success':
      return {
        ...state,
        availableBalance: {
          ...state.availableBalance,
          status: 'success',
          response: action.payload,
        },
      };
    case 'availableBalance/error':
      return {
        ...state,
        availableBalance: { ...state.availableBalance, status: 'error', error: action.payload },
      };
    default:
      return state;
  }
};
