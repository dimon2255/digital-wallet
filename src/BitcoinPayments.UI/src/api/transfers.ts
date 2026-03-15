import client from './client';

export interface TransferRequest {
  sourceWalletId: string;
  destinationWalletId: string;
  amountSatoshis: number;
  feeRateSatPerByte?: number;
}

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

export async function createTransfer(req: TransferRequest): Promise<PaymentResponse> {
  const { data } = await client.post<PaymentResponse>('/transfers', req, {
    headers: { 'Idempotency-Key': crypto.randomUUID() },
  });
  return data;
}
