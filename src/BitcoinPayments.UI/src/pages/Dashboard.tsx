import { Wallet, ArrowLeftRight, TrendingUp } from 'lucide-react';
import { useWallets } from '../hooks/useWallets';
import { useTransactions } from '../hooks/useTransactions';
import { formatSats, formatDate } from '../lib/formatters';
import StateBadge from '../components/common/StateBadge';
import { Link } from 'react-router-dom';

export default function Dashboard() {
  const { data: wallets } = useWallets();
  const { data: txPage } = useTransactions(1, 5);

  const totalBalance = wallets?.reduce((sum, w) => sum + w.balanceSatoshis, 0) ?? 0;

  return (
    <div>
      <h2 className="text-2xl font-semibold text-zinc-100 mb-6">Dashboard</h2>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
        <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-5">
          <div className="flex items-center gap-3 mb-3">
            <div className="p-2 bg-amber-500/10 rounded-lg">
              <TrendingUp size={18} className="text-amber-400" />
            </div>
            <span className="text-sm text-zinc-400">Total Balance</span>
          </div>
          <div className="text-2xl font-bold text-zinc-100 font-mono">{formatSats(totalBalance)}</div>
        </div>

        <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-5">
          <div className="flex items-center gap-3 mb-3">
            <div className="p-2 bg-amber-500/10 rounded-lg">
              <Wallet size={18} className="text-amber-400" />
            </div>
            <span className="text-sm text-zinc-400">Wallets</span>
          </div>
          <div className="text-2xl font-bold text-zinc-100">{wallets?.length ?? 0}</div>
        </div>

        <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-5">
          <div className="flex items-center gap-3 mb-3">
            <div className="p-2 bg-amber-500/10 rounded-lg">
              <ArrowLeftRight size={18} className="text-amber-400" />
            </div>
            <span className="text-sm text-zinc-400">Transactions</span>
          </div>
          <div className="text-2xl font-bold text-zinc-100">{txPage?.totalCount ?? 0}</div>
        </div>
      </div>

      <div className="bg-zinc-900 border border-zinc-800 rounded-xl">
        <div className="flex items-center justify-between p-5 border-b border-zinc-800">
          <h3 className="text-sm font-medium text-zinc-300">Recent Transactions</h3>
          <Link to="/transactions" className="text-xs text-amber-400 hover:text-amber-300">View all</Link>
        </div>
        {!txPage?.items.length ? (
          <p className="p-5 text-sm text-zinc-500">No transactions yet.</p>
        ) : (
          <div className="divide-y divide-zinc-800">
            {txPage.items.map((tx) => (
              <div key={tx.id} className="flex items-center justify-between px-5 py-3">
                <div>
                  <span className="text-sm text-zinc-200 capitalize">{tx.operationType}</span>
                  <span className="text-xs text-zinc-500 ml-2">{formatDate(tx.createdAt)}</span>
                </div>
                <div className="flex items-center gap-3">
                  <StateBadge state={tx.state} />
                  <span className="text-sm font-mono text-zinc-300">{formatSats(tx.amountSatoshis)}</span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
