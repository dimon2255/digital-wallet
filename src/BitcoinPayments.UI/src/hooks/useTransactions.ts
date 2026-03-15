import { useQuery } from '@tanstack/react-query';
import { listTransactions } from '../api/transactions';

export function useTransactions(page = 1, pageSize = 10) {
  return useQuery({
    queryKey: ['transactions', page, pageSize],
    queryFn: () => listTransactions(page, pageSize),
  });
}
