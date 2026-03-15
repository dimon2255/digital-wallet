import client from './client';

export interface WalletResponse {
  id: string;
  name: string;
  network: string;
  balanceSatoshis: number;
  currentReceivingIndex: number;
  currentChangeIndex: number;
  createdAt: string;
}

export interface WalletAddressResponse {
  walletId: string;
  address: string;
  derivationIndex: number;
}

export async function listWallets(): Promise<WalletResponse[]> {
  const { data } = await client.get<WalletResponse[]>('/wallets');
  return data;
}

export async function getWallet(id: string): Promise<WalletResponse> {
  const { data } = await client.get<WalletResponse>(`/wallets/${id}`);
  return data;
}

export async function createWallet(name: string): Promise<WalletResponse> {
  const { data } = await client.post<WalletResponse>('/wallets', { name }, {
    headers: { 'Idempotency-Key': crypto.randomUUID() },
  });
  return data;
}

export async function getWalletAddress(id: string): Promise<WalletAddressResponse> {
  const { data } = await client.get<WalletAddressResponse>(`/wallets/${id}/address`);
  return data;
}
