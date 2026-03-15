import client from './client';

export interface TransactionResponse {
  id: string;
  operationType: string;
  state: string;
  amountSatoshis: number;
  feeSatoshis: number | null;
  bitcoinTxId: string | null;
  confirmationCount: number;
  explorerUrl: string | null;
  createdAt: string;
  updatedAt: string;
  errorMessage: string | null;
  buyerWalletId: string;
  merchantWalletId: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function listTransactions(page = 1, pageSize = 10): Promise<PagedResult<TransactionResponse>> {
  const { data } = await client.get<PagedResult<TransactionResponse>>('/transactions', {
    params: { page, pageSize },
  });
  return data;
}
