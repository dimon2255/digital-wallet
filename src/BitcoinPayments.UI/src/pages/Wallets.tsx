import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Plus, Wallet as WalletIcon, ArrowRight, X } from 'lucide-react';
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
      {/* Header */}
      <div className="flex items-center justify-between mb-8 animate-fade-up">
        <div>
          <h2
            className="text-2xl font-bold text-zinc-100 tracking-tight"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Wallets
          </h2>
          <p className="text-sm text-zinc-500 mt-1" style={{ fontFamily: 'var(--font-body)' }}>
            Manage your Bitcoin wallets
          </p>
        </div>
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 px-4 py-2.5 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 btn-glow"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          <Plus size={16} />
          Create Wallet
        </button>
      </div>

      {/* Content */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="glass-card-solid p-5">
              <div className="skeleton h-10 w-10 rounded-xl mb-4" />
              <div className="skeleton h-4 w-24 mb-3" />
              <div className="skeleton h-7 w-36 mb-3" />
              <div className="skeleton h-3 w-40" />
            </div>
          ))}
        </div>
      ) : !wallets?.length ? (
        <div className="glass-card-solid p-14 text-center animate-fade-up delay-1">
          <div className="w-14 h-14 rounded-2xl bg-zinc-800/50 border border-zinc-700/30 flex items-center justify-center mx-auto mb-4">
            <WalletIcon size={24} className="text-zinc-600" />
          </div>
          <p
            className="text-sm text-zinc-400 mb-1"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            No wallets yet
          </p>
          <p className="text-xs text-zinc-600">
            Create your first wallet to start receiving Bitcoin
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {wallets.map((w, i) => (
            <Link
              key={w.id}
              to={`/wallets/${w.id}`}
              className={`glass-card-solid p-5 group animate-fade-up delay-${Math.min(i + 1, 6)}`}
            >
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/15 flex items-center justify-center group-hover:glow-amber-sm transition-all duration-300">
                    <WalletIcon size={18} className="text-amber-400" />
                  </div>
                  <h3
                    className="text-sm font-semibold text-zinc-200"
                    style={{ fontFamily: 'var(--font-display)' }}
                  >
                    {w.name}
                  </h3>
                </div>
                <ArrowRight
                  size={16}
                  className="text-zinc-700 group-hover:text-zinc-400 transition-colors duration-200"
                />
              </div>
              <div
                className="text-xl font-bold text-zinc-100 mb-2"
                style={{ fontFamily: 'var(--font-mono)' }}
              >
                {formatSats(w.balanceSatoshis)}
              </div>
              <div className="flex items-center gap-2 text-xs text-zinc-600" style={{ fontFamily: 'var(--font-body)' }}>
                <span className="px-1.5 py-0.5 rounded bg-zinc-800/60 text-zinc-500">{w.network}</span>
                <span>&middot;</span>
                <span>{formatDate(w.createdAt)}</span>
              </div>
            </Link>
          ))}
        </div>
      )}

      {/* Create Modal */}
      {showModal && (
        <div
          className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 animate-fade-in"
          style={{ backdropFilter: 'blur(4px)' }}
          onClick={() => setShowModal(false)}
        >
          <div
            className="glass-card-solid p-6 w-full max-w-sm animate-fade-up"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between mb-5">
              <h3
                className="text-lg font-semibold text-zinc-100"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                Create Wallet
              </h3>
              <button
                onClick={() => setShowModal(false)}
                className="p-1.5 rounded-lg text-zinc-500 hover:text-zinc-300 hover:bg-zinc-800/50 transition-colors"
              >
                <X size={16} />
              </button>
            </div>
            <form onSubmit={handleCreate}>
              <label className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase">
                Wallet Name
              </label>
              <input
                type="text"
                placeholder="e.g. Savings, Operations"
                value={name}
                onChange={(e) => setName(e.target.value)}
                autoFocus
                className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 placeholder:text-zinc-600 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all mb-5"
                style={{ fontFamily: 'var(--font-body)' }}
              />
              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="flex-1 py-2.5 bg-zinc-800/50 hover:bg-zinc-800 text-zinc-300 rounded-xl text-sm font-medium transition-colors"
                  style={{ fontFamily: 'var(--font-body)' }}
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createWallet.isPending}
                  className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 btn-glow"
                  style={{ fontFamily: 'var(--font-display)' }}
                >
                  {createWallet.isPending ? (
                    <span className="inline-flex items-center gap-2">
                      <span className="w-3.5 h-3.5 border-2 border-zinc-900/30 border-t-zinc-900 rounded-full animate-spin" />
                      Creating...
                    </span>
                  ) : (
                    'Create'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
