export function formatSats(satoshis: number): string {
  if (satoshis >= 100_000_000) {
    return `${(satoshis / 100_000_000).toFixed(8)} BTC`;
  }
  return `${satoshis.toLocaleString()} sats`;
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleString();
}

export function shortTxId(txId: string | null | undefined): string {
  if (!txId) return '-';
  return `${txId.slice(0, 8)}...${txId.slice(-8)}`;
}
