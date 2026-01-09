import {
  ApiResult,
  CreateCardPayload,
  CreateCardResponse,
  CreatePurchasePayload,
  CreatePurchaseResponse,
  FxRateRow,
} from '../types';

const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

const parseJson = async <T>(response: Response): Promise<T> => {
  const text = await response.text();
  if (!response.ok) {
    throw new Error(text || response.statusText);
  }

  return text ? (JSON.parse(text) as T) : ({} as T);
};

export const fetchFxRates = async (): Promise<FxRateRow[]> => {
  const response = await fetch(`${API_BASE}/fx-rates`);
  return parseJson<FxRateRow[]>(response);
};

export const createCard = async (payload: CreateCardPayload): Promise<CreateCardResponse> => {
  const response = await fetch(`${API_BASE}/cards`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  return parseJson<CreateCardResponse>(response);
};

export const createPurchase = async (
  cardId: string,
  payload: CreatePurchasePayload,
  idempotencyKey: string,
): Promise<CreatePurchaseResponse> => {
  const response = await fetch(`${API_BASE}/cards/${cardId}/purchases`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': idempotencyKey,
    },
    body: JSON.stringify(payload),
  });

  return parseJson<CreatePurchaseResponse>(response);
};

export const getPurchase = async (purchaseId: string, currency: string): Promise<ApiResult> => {
  const response = await fetch(
    `${API_BASE}/purchases/${purchaseId}?currency=${encodeURIComponent(currency)}`,
  );
  return parseJson<ApiResult>(response);
};

export const getAvailableBalance = async (cardId: string, currency: string): Promise<ApiResult> => {
  const response = await fetch(
    `${API_BASE}/cards/${cardId}/available-balance?currency=${encodeURIComponent(currency)}`,
  );
  return parseJson<ApiResult>(response);
};
