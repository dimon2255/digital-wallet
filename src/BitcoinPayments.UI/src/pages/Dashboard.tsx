import { Wallet, ArrowLeftRight, TrendingUp, ArrowRight, Inbox } from 'lucide-react';
import { useWallets } from '../hooks/useWallets';
import { useTransactions } from '../hooks/useTransactions';
import { formatSats, formatDate } from '../lib/formatters';
import StateBadge from '../components/common/StateBadge';
import { Link } from 'react-router';

const stats = [
  {
    key: 'balance',
    label: 'Total Balance',
    icon: TrendingUp,
    format: (v: number) => formatSats(v),
    mono: true,
  },
  {
    key: 'wallets',
    label: 'Wallets',
    icon: Wallet,
    format: (v: number) => v.toString(),
    mono: false,
  },
  {
    key: 'transactions',
    label: 'Transactions',
    icon: ArrowLeftRight,
    format: (v: number) => v.toString(),
    mono: false,
  },
] as const;

export default function Dashboard() {
  const { data: wallets } = useWallets();
  const { data: txPage } = useTransactions(1, 5);

  const totalBalance = wallets?.reduce((sum, w) => sum + w.balanceSatoshis, 0) ?? 0;

  const values: Record<string, number> = {
    balance: totalBalance,
    wallets: wallets?.length ?? 0,
    transactions: txPage?.totalCount ?? 0,
  };

  return (
    <div>
      {/* Page heading */}
      <div className="mb-8 animate-fade-up">
        <h2
          className="text-2xl font-bold text-zinc-100 tracking-tight"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Dashboard
        </h2>
        <p
          className="text-sm text-zinc-500 mt-1"
          style={{ fontFamily: 'var(--font-body)' }}
        >
          Overview of your Bitcoin payment operations
        </p>
      </div>

      {/* Stat cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        {stats.map(({ key, label, icon: Icon, format, mono }, i) => (
          <div
            key={key}
            className={`glass-card-solid p-5 group animate-fade-up delay-${i + 1}`}
          >
            <div className="flex items-center justify-between mb-4">
              <span
                className="text-sm text-zinc-500"
                style={{ fontFamily: 'var(--font-body)' }}
              >
                {label}
              </span>
              <div className="w-9 h-9 rounded-xl bg-amber-500/10 border border-amber-500/15 flex items-center justify-center group-hover:glow-amber-sm transition-all duration-300">
                <Icon size={16} className="text-amber-400" />
              </div>
            </div>
            <div
              className={`text-2xl font-bold text-zinc-100 ${mono ? 'font-mono' : ''}`}
              style={{ fontFamily: mono ? 'var(--font-mono)' : 'var(--font-display)' }}
            >
              {format(values[key])}
            </div>
          </div>
        ))}
      </div>

      {/* Recent Transactions */}
      <div className="glass-card-solid overflow-hidden animate-fade-up delay-4">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4">
          <h3
            className="text-sm font-semibold text-zinc-200"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Recent Transactions
          </h3>
          <Link
            to="/transactions"
            className="inline-flex items-center gap-1 text-xs font-medium text-amber-400 hover:text-amber-300 transition-colors"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            View all
            <ArrowRight size={12} />
          </Link>
        </div>

        {/* Divider */}
        <div className="h-px bg-gradient-to-r from-zinc-800/40 via-zinc-800 to-zinc-800/40" />

        {/* Content */}
        {!txPage?.items.length ? (
          <div className="flex flex-col items-center justify-center py-14 px-6">
            <div className="w-12 h-12 rounded-2xl bg-zinc-800/50 border border-zinc-700/30 flex items-center justify-center mb-4">
              <Inbox size={20} className="text-zinc-600" />
            </div>
            <p
              className="text-sm text-zinc-500"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              No transactions yet
            </p>
            <p className="text-xs text-zinc-600 mt-1">
              Create a wallet and start transacting
            </p>
          </div>
        ) : (
          <div>
            {txPage.items.map((tx, i) => (
              <Link
                key={tx.id}
                to={`/transactions/${tx.id}`}
                className={`flex items-center justify-between px-6 py-3.5 hover:bg-zinc-800/30 transition-colors cursor-pointer ${
                  i < txPage.items.length - 1 ? 'border-b border-zinc-800/50' : ''
                }`}
              >
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-lg bg-zinc-800/60 border border-zinc-700/30 flex items-center justify-center shrink-0">
                    <ArrowLeftRight size={14} className="text-zinc-500" />
                  </div>
                  <div>
                    <span
                      className="text-sm text-zinc-200 capitalize font-medium"
                      style={{ fontFamily: 'var(--font-body)' }}
                    >
                      {tx.operationType}
                    </span>
                    <span
                      className="text-xs text-zinc-600 ml-2"
                      style={{ fontFamily: 'var(--font-body)' }}
                    >
                      {formatDate(tx.createdAt)}
                    </span>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <StateBadge state={tx.state} />
                  <span
                    className="text-sm text-zinc-300 min-w-[90px] text-right"
                    style={{ fontFamily: 'var(--font-mono)' }}
                  >
                    {formatSats(tx.amountSatoshis)}
                  </span>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
