import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { listWallets, getWallet, createWallet, getWalletAddress } from '../api/wallets';

export function useWallets() {
  return useQuery({ queryKey: ['wallets'], queryFn: listWallets });
}

export function useWallet(id: string) {
  return useQuery({ queryKey: ['wallets', id], queryFn: () => getWallet(id), enabled: !!id });
}

export function useCreateWallet() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (name: string) => createWallet(name),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['wallets'] }),
  });
}

export function useWalletAddress(id: string) {
  return useQuery({
    queryKey: ['wallets', id, 'address'],
    queryFn: () => getWalletAddress(id),
    enabled: !!id,
    staleTime: 0,
  });
}
