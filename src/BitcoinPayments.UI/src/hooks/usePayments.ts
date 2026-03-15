import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  createCharge,
  createAuthorize,
  captureAuth,
  voidAuth,
  createRefund,
  getTransaction,
  getTransactionHistory,
  type ChargeRequest,
  type AuthorizeRequest,
  type CaptureRequest,
  type VoidRequest,
  type RefundRequest,
} from '../api/payments';

function usePaymentMutation<T>(fn: (req: T) => Promise<unknown>) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: fn,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['wallets'] });
      qc.invalidateQueries({ queryKey: ['transactions'] });
      qc.invalidateQueries({ queryKey: ['transaction'] });
    },
  });
}

export function useCreateCharge() {
  return usePaymentMutation<ChargeRequest>(createCharge);
}

export function useCreateAuthorize() {
  return usePaymentMutation<AuthorizeRequest>(createAuthorize);
}

export function useCaptureAuth() {
  return usePaymentMutation<CaptureRequest>(captureAuth);
}

export function useVoidAuth() {
  return usePaymentMutation<VoidRequest>(voidAuth);
}

export function useCreateRefund() {
  return usePaymentMutation<RefundRequest>(createRefund);
}

export function useTransaction(id: string) {
  return useQuery({
    queryKey: ['transaction', id],
    queryFn: () => getTransaction(id),
    enabled: !!id,
    refetchInterval: 15_000,
  });
}

export function useTransactionHistory(id: string) {
  return useQuery({
    queryKey: ['transaction', id, 'history'],
    queryFn: () => getTransactionHistory(id),
    enabled: !!id,
  });
}
