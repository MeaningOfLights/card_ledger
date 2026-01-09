export type RequestStatus = 'idle' | 'loading' | 'success' | 'error';

export type FxRateRow = {
  currency?: string;
  Currency?: string;
  rateDate?: string;
  RateDate?: string;
  usdToCurrency?: number;
  UsdToCurrency?: number;
};

export type CreateCardPayload = {
  cardNumber: string;
  creditLimit: number;
  currencyCode: string;
};

export type CreateCardResponse = {
  cardId: string;
};

export type CreatePurchasePayload = {
  description: string;
  transactionDate: string;
  amount: number;
  currencyCode: string;
};

export type CreatePurchaseResponse = {
  purchaseId: string;
  idempotencyKey: string;
};

export type ApiResult = Record<string, unknown>;
