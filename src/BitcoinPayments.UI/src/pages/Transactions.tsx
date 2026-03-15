import { useState } from 'react';
import { useTransactions } from '../hooks/useTransactions';
import { formatSats, formatDate, shortTxId } from '../lib/formatters';
import StateBadge from '../components/common/StateBadge';
import { ChevronLeft, ChevronRight, ExternalLink, Inbox } from 'lucide-react';

export default function Transactions() {
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const { data, isLoading } = useTransactions(page, pageSize);

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      {/* Header */}
      <div className="mb-8 animate-fade-up">
        <h2
          className="text-2xl font-bold text-zinc-100 tracking-tight"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Transaction History
        </h2>
        <p className="text-sm text-zinc-500 mt-1" style={{ fontFamily: 'var(--font-body)' }}>
          All payment operations across your wallets
        </p>
      </div>

      {/* Table Card */}
      <div className="glass-card-solid overflow-hidden animate-fade-up delay-1">
        {isLoading ? (
          <div className="p-6 space-y-3">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex items-center justify-between">
                <div className="flex items-center gap-3">
                  <div className="skeleton h-4 w-20" />
                  <div className="skeleton h-6 w-16 rounded-lg" />
                </div>
                <div className="flex items-center gap-4">
                  <div className="skeleton h-4 w-24" />
                  <div className="skeleton h-4 w-28" />
                </div>
              </div>
            ))}
          </div>
        ) : !data?.items.length ? (
          <div className="flex flex-col items-center justify-center py-16 px-6">
            <div className="w-12 h-12 rounded-2xl bg-zinc-800/50 border border-zinc-700/30 flex items-center justify-center mb-4">
              <Inbox size={20} className="text-zinc-600" />
            </div>
            <p className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
              No transactions found
            </p>
            <p className="text-xs text-zinc-600 mt-1">
              Transactions will appear here once you start transacting
            </p>
          </div>
        ) : (
          <>
            {/* Table header */}
            <div className="hidden sm:grid grid-cols-[1fr_auto_1fr_1fr_1.2fr_1fr] gap-2 px-6 py-3 text-xs font-medium text-zinc-600 uppercase tracking-wider border-b border-zinc-800/50" style={{ fontFamily: 'var(--font-body)' }}>
              <span>Type</span>
              <span>State</span>
              <span className="text-right">Amount</span>
              <span className="text-right">Fee</span>
              <span>Tx ID</span>
              <span>Date</span>
            </div>

            {/* Rows */}
            <div>
              {data.items.map((tx, i) => (
                <div
                  key={tx.id}
                  className={`grid grid-cols-1 sm:grid-cols-[1fr_auto_1fr_1fr_1.2fr_1fr] gap-2 items-center px-6 py-3.5 hover:bg-zinc-800/20 transition-colors ${
                    i < data.items.length - 1 ? 'border-b border-zinc-800/30' : ''
                  }`}
                >
                  {/* Type */}
                  <span
                    className="text-sm text-zinc-200 capitalize font-medium"
                    style={{ fontFamily: 'var(--font-body)' }}
                  >
                    {tx.operationType}
                  </span>

                  {/* State */}
                  <div>
                    <StateBadge state={tx.state} />
                  </div>

                  {/* Amount */}
                  <span
                    className="text-sm text-zinc-200 sm:text-right"
                    style={{ fontFamily: 'var(--font-mono)' }}
                  >
                    {formatSats(tx.amountSatoshis)}
                  </span>

                  {/* Fee */}
                  <span
                    className="text-sm text-zinc-500 sm:text-right"
                    style={{ fontFamily: 'var(--font-mono)' }}
                  >
                    {tx.feeSatoshis != null ? formatSats(tx.feeSatoshis) : '-'}
                  </span>

                  {/* Tx ID */}
                  <div>
                    {tx.bitcoinTxId ? (
                      tx.explorerUrl ? (
                        <a
                          href={tx.explorerUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="inline-flex items-center gap-1 text-amber-400 hover:text-amber-300 text-xs transition-colors"
                          style={{ fontFamily: 'var(--font-mono)' }}
                        >
                          {shortTxId(tx.bitcoinTxId)}
                          <ExternalLink size={11} />
                        </a>
                      ) : (
                        <span className="text-xs text-zinc-500" style={{ fontFamily: 'var(--font-mono)' }}>
                          {shortTxId(tx.bitcoinTxId)}
                        </span>
                      )
                    ) : (
                      <span className="text-zinc-600">-</span>
                    )}
                  </div>

                  {/* Date */}
                  <span
                    className="text-xs text-zinc-500"
                    style={{ fontFamily: 'var(--font-body)' }}
                  >
                    {formatDate(tx.createdAt)}
                  </span>
                </div>
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="flex items-center justify-between px-6 py-3.5 border-t border-zinc-800/50">
                <span className="text-xs text-zinc-600" style={{ fontFamily: 'var(--font-body)' }}>
                  Page {page} of {totalPages} &middot; {data.totalCount} total
                </span>
                <div className="flex items-center gap-1.5">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page === 1}
                    className="p-1.5 rounded-lg bg-zinc-800/40 hover:bg-zinc-800 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                  >
                    <ChevronLeft size={16} className="text-zinc-400" />
                  </button>
                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page === totalPages}
                    className="p-1.5 rounded-lg bg-zinc-800/40 hover:bg-zinc-800 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
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
