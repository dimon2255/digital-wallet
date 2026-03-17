import { useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router';
import { useWallets } from '../hooks/useWallets';
import { useCreateCharge, useCreateAuthorize } from '../hooks/usePayments';
import { useCreateTransfer } from '../hooks/useTransfers';
import { formatSats } from '../lib/formatters';
import toast from 'react-hot-toast';
import { Send, Shield, Zap, ArrowRight, AlertTriangle, X } from 'lucide-react';

type TabId = 'charge' | 'authorize' | 'transfer';

const tabs: { id: TabId; label: string; icon: typeof Send }[] = [
  { id: 'charge', label: 'Charge', icon: Zap },
  { id: 'authorize', label: 'Authorize', icon: Shield },
  { id: 'transfer', label: 'Transfer', icon: Send },
];

export default function PaymentsPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { data: wallets } = useWallets();
  const chargeMutation = useCreateCharge();
  const authMutation = useCreateAuthorize();
  const transferMutation = useCreateTransfer();

  const [activeTab, setActiveTab] = useState<TabId>(
    (searchParams.get('tab') as TabId) || 'charge'
  );
  const [buyerWalletId, setBuyerWalletId] = useState(searchParams.get('from') || '');
  const [merchantWalletId, setMerchantWalletId] = useState('');
  const [amount, setAmount] = useState('');
  const [authWindowBlocks, setAuthWindowBlocks] = useState('144');
  const [showConfirm, setShowConfirm] = useState(false);

  const amountSats = parseInt(amount) || 0;
  const sourceWallet = wallets?.find((w) => w.id === buyerWalletId);
  const insufficientBalance = sourceWallet && amountSats > sourceWallet.balanceSatoshis;
  const isPending = chargeMutation.isPending || authMutation.isPending || transferMutation.isPending;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!buyerWalletId || !merchantWalletId || amountSats <= 0) {
      toast.error('Fill all fields');
      return;
    }
    if (buyerWalletId === merchantWalletId) {
      toast.error('Source and destination must be different');
      return;
    }
    setShowConfirm(true);
  };

  const handleConfirm = async () => {
    try {
      if (activeTab === 'charge') {
        await chargeMutation.mutateAsync({
          buyerWalletId,
          merchantWalletId,
          amountSatoshis: amountSats,
        });
        toast.success('Charge submitted');
      } else if (activeTab === 'authorize') {
        await authMutation.mutateAsync({
          buyerWalletId,
          merchantWalletId,
          amountSatoshis: amountSats,
          authWindowBlocks: parseInt(authWindowBlocks) || 144,
        });
        toast.success('Authorization submitted — escrow created');
      } else {
        await transferMutation.mutateAsync({
          sourceWalletId: buyerWalletId,
          destinationWalletId: merchantWalletId,
          amountSatoshis: amountSats,
        });
        toast.success('Transfer submitted');
      }
      navigate('/transactions');
    } catch (err: any) {
      toast.error(err.response?.data?.error?.message || 'Operation failed');
    }
    setShowConfirm(false);
  };

  const resetForm = () => {
    setBuyerWalletId('');
    setMerchantWalletId('');
    setAmount('');
    setAuthWindowBlocks('144');
  };

  return (
    <div className="max-w-lg">
      {/* Header */}
      <div className="mb-8 animate-fade-up">
        <h2
          className="text-2xl font-bold text-zinc-100 tracking-tight"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Payments
        </h2>
        <p className="text-sm text-zinc-500 mt-1" style={{ fontFamily: 'var(--font-body)' }}>
          Create charges, authorizations, and transfers
        </p>
      </div>

      {/* Tab switcher */}
      <div className="flex mb-6 bg-zinc-900 rounded-xl p-1 border border-zinc-800/60 animate-fade-up delay-1">
        {tabs.map(({ id, label, icon: Icon }) => (
          <button
            key={id}
            onClick={() => { setActiveTab(id); resetForm(); }}
            className={`flex-1 flex items-center justify-center gap-2 py-2.5 text-sm rounded-lg font-medium transition-all duration-200 ${
              activeTab === id
                ? 'bg-zinc-800 text-zinc-100 shadow-sm'
                : 'text-zinc-500 hover:text-zinc-300'
            }`}
            style={{ fontFamily: 'var(--font-body)' }}
          >
            <Icon size={14} />
            {label}
          </button>
        ))}
      </div>

      {/* Tab description */}
      <div className="glass-card-solid p-4 mb-4 animate-fade-up delay-2">
        {activeTab === 'charge' && (
          <div className="flex items-start gap-3">
            <Zap size={16} className="text-amber-400 shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-medium text-zinc-200" style={{ fontFamily: 'var(--font-display)' }}>
                Direct Charge
              </p>
              <p className="text-xs text-zinc-500 mt-1 leading-relaxed" style={{ fontFamily: 'var(--font-body)' }}>
                Formal payment from buyer to merchant — <span className="text-zinc-300">always settles on-chain</span>, even between your own wallets. Creates a payment record that supports <span className="text-amber-400">refunds</span>. Use this when you need an auditable payment trail.
              </p>
            </div>
          </div>
        )}
        {activeTab === 'authorize' && (
          <div className="flex items-start gap-3">
            <Shield size={16} className="text-blue-400 shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-medium text-zinc-200" style={{ fontFamily: 'var(--font-display)' }}>
                Authorization Hold
              </p>
              <p className="text-xs text-zinc-500 mt-1 leading-relaxed" style={{ fontFamily: 'var(--font-body)' }}>
                Funds are locked in a 2-of-2 multisig escrow on-chain. You can <span className="text-green-400">capture</span> (release to merchant) or <span className="text-zinc-300">void</span> (return to buyer) before the auth window expires. Non-custodial by design.
              </p>
            </div>
          </div>
        )}
        {activeTab === 'transfer' && (
          <div className="flex items-start gap-3">
            <Send size={16} className="text-amber-400 shrink-0 mt-0.5" />
            <div>
              <p className="text-sm font-medium text-zinc-200" style={{ fontFamily: 'var(--font-display)' }}>
                Wallet Transfer
              </p>
              <p className="text-xs text-zinc-500 mt-1 leading-relaxed" style={{ fontFamily: 'var(--font-body)' }}>
                Move Bitcoin between wallets. <span className="text-green-400">Same-user = instant</span> via internal ledger (no fees, no on-chain tx). Different users = on-chain settlement. Best for moving your own funds around.
              </p>
            </div>
          </div>
        )}
      </div>

      {/* Form */}
      <form onSubmit={handleSubmit} className="glass-card-solid p-6 space-y-5 animate-fade-up delay-3">
        {/* Source wallet */}
        <div>
          <label
            className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            {activeTab === 'transfer' ? 'From Wallet' : 'Buyer Wallet'}
          </label>
          <select
            value={buyerWalletId}
            onChange={(e) => setBuyerWalletId(e.target.value)}
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

        {/* Arrow */}
        <div className="flex justify-center">
          <div className="w-8 h-8 rounded-lg bg-zinc-800/50 border border-zinc-700/30 flex items-center justify-center">
            <ArrowRight size={14} className="text-zinc-500 rotate-90" />
          </div>
        </div>

        {/* Destination wallet */}
        <div>
          <label
            className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            {activeTab === 'transfer' ? 'To Wallet' : 'Merchant Wallet'}
          </label>
          <select
            value={merchantWalletId}
            onChange={(e) => setMerchantWalletId(e.target.value)}
            className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all appearance-none cursor-pointer"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            <option value="">Select wallet...</option>
            {wallets?.filter((w) => w.id !== buyerWalletId).map((w) => (
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

        {/* Auth window (authorize only) */}
        {activeTab === 'authorize' && (
          <div>
            <label
              className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              Auth Window (blocks)
            </label>
            <input
              type="number"
              value={authWindowBlocks}
              onChange={(e) => setAuthWindowBlocks(e.target.value)}
              placeholder="144"
              min="1"
              className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 placeholder:text-zinc-600 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all"
              style={{ fontFamily: 'var(--font-mono)' }}
            />
            <p className="text-xs text-zinc-600 mt-1.5">
              ~{Math.round((parseInt(authWindowBlocks) || 144) * 10 / 60)} hours ({parseInt(authWindowBlocks) || 144} blocks x 10min)
            </p>
          </div>
        )}

        {/* Authorize info badge */}
        {activeTab === 'authorize' && (
          <div className="flex items-start gap-2 px-3 py-2.5 bg-blue-500/8 border border-blue-500/15 rounded-xl">
            <Shield size={14} className="text-blue-400 shrink-0 mt-0.5" />
            <span className="text-xs text-blue-400 leading-relaxed">
              Creates a 2-of-2 multisig escrow. Funds are held on-chain until you capture or void.
            </span>
          </div>
        )}

        {/* Insufficient balance */}
        {insufficientBalance && (
          <div className="flex items-center gap-2 px-3 py-2 bg-red-500/8 border border-red-500/15 rounded-xl">
            <AlertTriangle size={14} className="text-red-400 shrink-0" />
            <span className="text-xs text-red-400">Insufficient balance</span>
          </div>
        )}

        {/* Submit */}
        <button
          type="submit"
          disabled={isPending}
          className="w-full py-3 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed btn-glow"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          <span className="inline-flex items-center gap-2">
            {tabs.find((t) => t.id === activeTab)?.icon && (() => {
              const Icon = tabs.find((t) => t.id === activeTab)!.icon;
              return <Icon size={15} />;
            })()}
            Review {activeTab === 'charge' ? 'Charge' : activeTab === 'authorize' ? 'Authorization' : 'Transfer'}
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
                className="text-lg font-semibold text-zinc-100 capitalize"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                Confirm {activeTab}
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
                { label: 'Operation', value: activeTab.charAt(0).toUpperCase() + activeTab.slice(1) },
                { label: 'Amount', value: formatSats(amountSats), mono: true },
                { label: activeTab === 'transfer' ? 'From' : 'Buyer', value: sourceWallet?.name || '-' },
                { label: activeTab === 'transfer' ? 'To' : 'Merchant', value: wallets?.find((w) => w.id === merchantWalletId)?.name || '-' },
                ...(activeTab === 'authorize'
                  ? [{ label: 'Auth Window', value: `${authWindowBlocks} blocks` }]
                  : []),
              ].map(({ label, value, mono }) => (
                <div key={label} className="flex justify-between items-center py-2 border-b border-zinc-800/50 last:border-0">
                  <span className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>{label}</span>
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
                disabled={isPending}
                className="flex-1 py-2.5 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 btn-glow"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                {isPending ? (
                  <span className="inline-flex items-center gap-2">
                    <span className="w-3.5 h-3.5 border-2 border-zinc-900/30 border-t-zinc-900 rounded-full animate-spin" />
                    Processing...
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
