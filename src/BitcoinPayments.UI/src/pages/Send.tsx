import { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { useWallets } from '../hooks/useWallets';
import { useCreateTransfer } from '../hooks/useTransfers';
import { formatSats } from '../lib/formatters';
import toast from 'react-hot-toast';

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

  return (
    <div className="max-w-lg">
      <h2 className="text-2xl font-semibold text-zinc-100 mb-6">Send Bitcoin</h2>

      <form onSubmit={handleSubmit} className="bg-zinc-900 border border-zinc-800 rounded-xl p-6 space-y-5">
        <div>
          <label className="block text-sm text-zinc-400 mb-1.5">From Wallet</label>
          <select
            value={sourceId}
            onChange={(e) => setSourceId(e.target.value)}
            className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 focus:outline-none focus:border-amber-500"
          >
            <option value="">Select wallet...</option>
            {wallets?.map((w) => (
              <option key={w.id} value={w.id}>
                {w.name} ({formatSats(w.balanceSatoshis)})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm text-zinc-400 mb-1.5">To Wallet</label>
          <select
            value={destId}
            onChange={(e) => setDestId(e.target.value)}
            className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 focus:outline-none focus:border-amber-500"
          >
            <option value="">Select wallet...</option>
            {wallets?.filter((w) => w.id !== sourceId).map((w) => (
              <option key={w.id} value={w.id}>
                {w.name} ({formatSats(w.balanceSatoshis)})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm text-zinc-400 mb-1.5">Amount (satoshis)</label>
          <input
            type="number"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            placeholder="10000"
            min="1"
            className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 placeholder:text-zinc-500 focus:outline-none focus:border-amber-500"
          />
          {amountSats > 0 && (
            <p className="text-xs text-zinc-500 mt-1">{formatSats(amountSats)}</p>
          )}
        </div>

        {sourceWallet && amountSats > sourceWallet.balanceSatoshis && (
          <p className="text-xs text-red-400">Insufficient balance</p>
        )}

        <button
          type="submit"
          disabled={transfer.isPending}
          className="w-full py-2.5 bg-amber-500 hover:bg-amber-600 text-zinc-900 font-medium rounded-lg text-sm transition-colors disabled:opacity-50"
        >
          Review Transfer
        </button>
      </form>

      {showConfirm && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50" onClick={() => setShowConfirm(false)}>
          <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-6 w-full max-w-sm" onClick={(e) => e.stopPropagation()}>
            <h3 className="text-lg font-medium text-zinc-100 mb-4">Confirm Transfer</h3>
            <div className="space-y-2 text-sm mb-6">
              <div className="flex justify-between">
                <span className="text-zinc-400">Amount</span>
                <span className="text-zinc-200 font-mono">{formatSats(amountSats)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-zinc-400">From</span>
                <span className="text-zinc-200">{sourceWallet?.name}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-zinc-400">To</span>
                <span className="text-zinc-200">{wallets?.find((w) => w.id === destId)?.name}</span>
              </div>
            </div>
            <div className="flex gap-3">
              <button onClick={() => setShowConfirm(false)} className="flex-1 py-2 bg-zinc-800 text-zinc-300 rounded-lg text-sm">
                Cancel
              </button>
              <button onClick={handleConfirm} disabled={transfer.isPending} className="flex-1 py-2 bg-amber-500 text-zinc-900 font-medium rounded-lg text-sm disabled:opacity-50">
                {transfer.isPending ? 'Sending...' : 'Confirm'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
