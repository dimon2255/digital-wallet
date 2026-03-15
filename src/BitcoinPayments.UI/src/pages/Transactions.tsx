import { useState } from 'react';
import { useTransactions } from '../hooks/useTransactions';
import { formatSats, formatDate, shortTxId } from '../lib/formatters';
import StateBadge from '../components/common/StateBadge';
import { ChevronLeft, ChevronRight, ExternalLink } from 'lucide-react';

export default function Transactions() {
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const { data, isLoading } = useTransactions(page, pageSize);

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <h2 className="text-2xl font-semibold text-zinc-100 mb-6">Transaction History</h2>

      <div className="bg-zinc-900 border border-zinc-800 rounded-xl overflow-hidden">
        {isLoading ? (
          <p className="p-5 text-zinc-500">Loading...</p>
        ) : !data?.items.length ? (
          <p className="p-5 text-sm text-zinc-500">No transactions found.</p>
        ) : (
          <>
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-zinc-800">
                  <th className="text-left px-5 py-3 text-zinc-500 font-medium">Type</th>
                  <th className="text-left px-5 py-3 text-zinc-500 font-medium">State</th>
                  <th className="text-right px-5 py-3 text-zinc-500 font-medium">Amount</th>
                  <th className="text-right px-5 py-3 text-zinc-500 font-medium">Fee</th>
                  <th className="text-left px-5 py-3 text-zinc-500 font-medium">Tx ID</th>
                  <th className="text-left px-5 py-3 text-zinc-500 font-medium">Date</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-800">
                {data.items.map((tx) => (
                  <tr key={tx.id} className="hover:bg-zinc-800/50">
                    <td className="px-5 py-3 text-zinc-200 capitalize">{tx.operationType}</td>
                    <td className="px-5 py-3"><StateBadge state={tx.state} /></td>
                    <td className="px-5 py-3 text-right font-mono text-zinc-200">{formatSats(tx.amountSatoshis)}</td>
                    <td className="px-5 py-3 text-right font-mono text-zinc-400">
                      {tx.feeSatoshis != null ? formatSats(tx.feeSatoshis) : '-'}
                    </td>
                    <td className="px-5 py-3">
                      {tx.bitcoinTxId ? (
                        tx.explorerUrl ? (
                          <a href={tx.explorerUrl} target="_blank" rel="noopener noreferrer" className="flex items-center gap-1 text-amber-400 hover:text-amber-300 font-mono text-xs">
                            {shortTxId(tx.bitcoinTxId)}
                            <ExternalLink size={12} />
                          </a>
                        ) : (
                          <span className="font-mono text-xs text-zinc-400">{shortTxId(tx.bitcoinTxId)}</span>
                        )
                      ) : (
                        <span className="text-zinc-500">-</span>
                      )}
                    </td>
                    <td className="px-5 py-3 text-zinc-400 text-xs">{formatDate(tx.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            {totalPages > 1 && (
              <div className="flex items-center justify-between px-5 py-3 border-t border-zinc-800">
                <span className="text-xs text-zinc-500">
                  Page {page} of {totalPages} ({data.totalCount} total)
                </span>
                <div className="flex gap-2">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page === 1}
                    className="p-1.5 bg-zinc-800 rounded disabled:opacity-30"
                  >
                    <ChevronLeft size={16} className="text-zinc-400" />
                  </button>
                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page === totalPages}
                    className="p-1.5 bg-zinc-800 rounded disabled:opacity-30"
                  >
                    <ChevronRight size={16} className="text-zinc-400" />
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
