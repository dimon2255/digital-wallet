import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Wallet as WalletIcon } from 'lucide-react';
import { useWallets, useCreateWallet } from '../hooks/useWallets';
import { formatSats, formatDate } from '../lib/formatters';
import toast from 'react-hot-toast';

export default function Wallets() {
  const { data: wallets, isLoading } = useWallets();
  const createWallet = useCreateWallet();
  const [showModal, setShowModal] = useState(false);
  const [name, setName] = useState('');

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    try {
      await createWallet.mutateAsync(name.trim());
      setShowModal(false);
      setName('');
      toast.success('Wallet created');
    } catch {
      toast.error('Failed to create wallet');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-semibold text-zinc-100">Wallets</h2>
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 px-4 py-2 bg-amber-500 hover:bg-amber-600 text-zinc-900 font-medium rounded-lg text-sm transition-colors"
        >
          <Plus size={16} />
          Create Wallet
        </button>
      </div>

      {isLoading ? (
        <p className="text-zinc-500">Loading...</p>
      ) : !wallets?.length ? (
        <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-12 text-center">
          <WalletIcon size={48} className="mx-auto text-zinc-600 mb-4" />
          <p className="text-zinc-400">No wallets yet. Create one to get started.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {wallets.map((w) => (
            <Link
              key={w.id}
              to={`/wallets/${w.id}`}
              className="bg-zinc-900 border border-zinc-800 rounded-xl p-5 hover:border-zinc-700 transition-colors"
            >
              <div className="flex items-center gap-3 mb-3">
                <div className="p-2 bg-amber-500/10 rounded-lg">
                  <WalletIcon size={18} className="text-amber-400" />
                </div>
                <h3 className="text-sm font-medium text-zinc-200">{w.name}</h3>
              </div>
              <div className="text-xl font-bold text-zinc-100 font-mono mb-2">{formatSats(w.balanceSatoshis)}</div>
              <div className="text-xs text-zinc-500">
                {w.network} &middot; Created {formatDate(w.createdAt)}
              </div>
            </Link>
          ))}
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50" onClick={() => setShowModal(false)}>
          <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-6 w-full max-w-sm" onClick={(e) => e.stopPropagation()}>
            <h3 className="text-lg font-medium text-zinc-100 mb-4">Create Wallet</h3>
            <form onSubmit={handleCreate}>
              <input
                type="text"
                placeholder="Wallet name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                autoFocus
                className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 placeholder:text-zinc-500 focus:outline-none focus:border-amber-500 mb-4"
              />
              <div className="flex gap-3">
                <button type="button" onClick={() => setShowModal(false)} className="flex-1 py-2 bg-zinc-800 text-zinc-300 rounded-lg text-sm">
                  Cancel
                </button>
                <button type="submit" disabled={createWallet.isPending} className="flex-1 py-2 bg-amber-500 text-zinc-900 font-medium rounded-lg text-sm disabled:opacity-50">
                  {createWallet.isPending ? '...' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
