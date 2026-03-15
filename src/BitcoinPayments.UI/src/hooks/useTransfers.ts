import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createTransfer, type TransferRequest } from '../api/transfers';

export function useCreateTransfer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (req: TransferRequest) => createTransfer(req),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['wallets'] });
      qc.invalidateQueries({ queryKey: ['transactions'] });
    },
  });
}
