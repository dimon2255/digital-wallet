import client from './client';
import type { TransactionResponse } from './transactions';

// ——— Request types ———

export interface ChargeRequest {
  buyerWalletId: string;
  merchantWalletId: string;
  amountSatoshis: number;
  feeRateSatPerByte?: number;
  metadata?: Record<string, string>;
}

export interface AuthorizeRequest {
  buyerWalletId: string;
  merchantWalletId: string;
  amountSatoshis: number;
  authWindowBlocks?: number;
  feeRateSatPerByte?: number;
  metadata?: Record<string, string>;
}

export interface CaptureRequest {
  authorizationId: string;
  amountSatoshis: number;
  feeRateSatPerByte?: number;
  metadata?: Record<string, string>;
}

export interface VoidRequest {
  authorizationId: string;
  feeRateSatPerByte?: number;
  metadata?: Record<string, string>;
}

export interface RefundRequest {
  parentTransactionId: string;
  amountSatoshis: number;
  feeRateSatPerByte?: number;
  metadata?: Record<string, string>;
}

// ——— Response types ———

export interface PaymentResponse {
  id: string;
  operationType: string;
  state: string;
  amountSatoshis: number;
  feeSatoshis: number | null;
  bitcoinTxId: string | null;
  confirmationCount: number;
  explorerUrl: string | null;
  createdAt: string;
}

export interface AuthorizationResponse extends PaymentResponse {
  escrowAddress: string;
  expiresAtBlock: number | null;
}

// ——— Idempotency helper ———

function idempotencyHeaders() {
  return { 'Idempotency-Key': crypto.randomUUID() };
}

// ——— API calls ———

export async function createCharge(req: ChargeRequest): Promise<PaymentResponse> {
  const { data } = await client.post<PaymentResponse>('/payments/charge', req, {
    headers: idempotencyHeaders(),
  });
  return data;
}

export async function createAuthorize(req: AuthorizeRequest): Promise<AuthorizationResponse> {
  const { data } = await client.post<AuthorizationResponse>('/payments/authorize', req, {
    headers: idempotencyHeaders(),
  });
  return data;
}

export async function captureAuth(req: CaptureRequest): Promise<PaymentResponse> {
  const { data } = await client.post<PaymentResponse>('/payments/capture', req, {
    headers: idempotencyHeaders(),
  });
  return data;
}

export async function voidAuth(req: VoidRequest): Promise<PaymentResponse> {
  const { data } = await client.post<PaymentResponse>('/payments/void', req, {
    headers: idempotencyHeaders(),
  });
  return data;
}

export async function createRefund(req: RefundRequest): Promise<PaymentResponse> {
  const { data } = await client.post<PaymentResponse>('/payments/refund', req, {
    headers: idempotencyHeaders(),
  });
  return data;
}

export async function getTransaction(id: string): Promise<TransactionResponse> {
  const { data } = await client.get<TransactionResponse>(`/payments/${id}`);
  return data;
}

export async function getTransactionHistory(id: string): Promise<TransactionResponse[]> {
  const { data } = await client.get<TransactionResponse[]>(`/payments/${id}/history`);
  return data;
}
