import { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router';
import { useWallets } from '../hooks/useWallets';
import { useCreateTransfer } from '../hooks/useTransfers';
import { formatSats } from '../lib/formatters';
import toast from 'react-hot-toast';
import { Send as SendIcon, ArrowRight, X, AlertTriangle } from 'lucide-react';

export default function Send() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { data: wallets } = useWallets();
  const transfer = useCreateTransfer();

  const [sourceId, setSourceId] = useState(searchParams.get('from') || '');
  const [destId, setDestId] = useState('');
  const [amount, setAmount] = useState('');
  const [showConfirm, setShowConfirm] = useState(false);

  const sourceWallet = wallets?.find((w) => w.id === sourceId);
  const amountSats = parseInt(amount) || 0;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!sourceId || !destId || amountSats <= 0) {
      toast.error('Fill all fields');
      return;
    }
    if (sourceId === destId) {
      toast.error('Source and destination must be different');
      return;
    }
    setShowConfirm(true);
  };

  const handleConfirm = async () => {
    try {
      await transfer.mutateAsync({
        sourceWalletId: sourceId,
        destinationWalletId: destId,
        amountSatoshis: amountSats,
      });
      toast.success('Transfer submitted');
      navigate('/transactions');
    } catch (err: any) {
      toast.error(err.response?.data?.error?.message || 'Transfer failed');
    }
    setShowConfirm(false);
  };

  const insufficientBalance = sourceWallet && amountSats > sourceWallet.balanceSatoshis;

  return (
    <div className="max-w-lg">
      {/* Header */}
      <div className="mb-8 animate-fade-up">
        <h2
          className="text-2xl font-bold text-zinc-100 tracking-tight"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Send Bitcoin
        </h2>
        <p className="text-sm text-zinc-500 mt-1" style={{ fontFamily: 'var(--font-body)' }}>
          Transfer between your wallets
        </p>
      </div>

      {/* Form Card */}
      <form onSubmit={handleSubmit} className="glass-card-solid p-6 space-y-5 animate-fade-up delay-1">
        {/* From */}
        <div>
          <label
            className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            From Wallet
          </label>
          <select
            value={sourceId}
            onChange={(e) => setSourceId(e.target.value)}
            className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all appearance-none cursor-pointer"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            <option value="">Select wallet...</option>
            {wallets?.map((w) => (
              <option key={w.id} value={w.id}>
                {w.name} ({formatSats(w.balanceSatoshis)})
              </option>
            ))}
          </select>
        </div>

        {/* Arrow separator */}
        <div className="flex justify-center">
          <div className="w-8 h-8 rounded-lg bg-zinc-800/50 border border-zinc-700/30 flex items-center justify-center">
            <ArrowRight size={14} className="text-zinc-500 rotate-90" />
          </div>
        </div>

        {/* To */}
        <div>
          <label
            className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            To Wallet
          </label>
          <select
            value={destId}
            onChange={(e) => setDestId(e.target.value)}
            className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all appearance-none cursor-pointer"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            <option value="">Select wallet...</option>
            {wallets?.filter((w) => w.id !== sourceId).map((w) => (
              <option key={w.id} value={w.id}>
                {w.name} ({formatSats(w.balanceSatoshis)})
              </option>
            ))}
          </select>
        </div>

        {/* Amount */}
        <div>
          <label
            className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            Amount (satoshis)
          </label>
          <input
            type="number"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            placeholder="10000"
            min="1"
            className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 placeholder:text-zinc-600 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all"
            style={{ fontFamily: 'var(--font-mono)' }}
          />
          {amountSats > 0 && (
            <p className="text-xs text-zinc-500 mt-1.5" style={{ fontFamily: 'var(--font-mono)' }}>
              {formatSats(amountSats)}
            </p>
          )}
        </div>

        {/* Insufficient balance warning */}
        {insufficientBalance && (
          <div className="flex items-center gap-2 px-3 py-2 bg-red-500/8 border border-red-500/15 rounded-xl">
            <AlertTriangle size={14} className="text-red-400 shrink-0" />
            <span className="text-xs text-red-400" style={{ fontFamily: 'var(--font-body)' }}>
              Insufficient balance
            </span>
          </div>
        )}

        {/* Submit */}
        <button
          type="submit"
          disabled={transfer.isPending}
          className="w-full py-3 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed btn-glow"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          <span className="inline-flex items-center gap-2">
            <SendIcon size={15} />
            Review Transfer
          </span>
        </button>
      </form>

      {/* Confirmation Modal */}
      {showConfirm && (
        <div
          className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 animate-fade-in"
          style={{ backdropFilter: 'blur(4px)' }}
          onClick={() => setShowConfirm(false)}
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
                Confirm Transfer
              </h3>
              <button
                onClick={() => setShowConfirm(false)}
                className="p-1.5 rounded-lg text-zinc-500 hover:text-zinc-300 hover:bg-zinc-800/50 transition-colors"
              >
                <X size={16} />
              </button>
            </div>

            <div className="space-y-3 mb-6">
              {[
                { label: 'Amount', value: formatSats(amountSats), mono: true },
                { label: 'From', value: sourceWallet?.name || '-', mono: false },
                { label: 'To', value: wallets?.find((w) => w.id === destId)?.name || '-', mono: false },
              ].map(({ label, value, mono }) => (
                <div key={label} className="flex justify-between items-center py-2 border-b border-zinc-800/50 last:border-0">
                  <span className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
                    {label}
                  </span>
                  <span
                    className="text-sm font-medium text-zinc-200"
                    style={{ fontFamily: mono ? 'var(--font-mono)' : 'var(--font-body)' }}
                  >
                    {value}
                  </span>
                </div>
              ))}
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => setShowConfirm(false)}
                className="flex-1 py-2.5 bg-zinc-800/50 hover:bg-zinc-800 text-zinc-300 rounded-xl text-sm font-medium transition-colors"
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Cancel
              </button>
              <button
                onClick={handleConfirm}
                disabled={transfer.isPending}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 btn-glow"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                {transfer.isPending ? (
                  <span className="inline-flex items-center gap-2">
                    <span className="w-3.5 h-3.5 border-2 border-zinc-900/30 border-t-zinc-900 rounded-full animate-spin" />
                    Sending...
                  </span>
                ) : (
                  'Confirm'
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
