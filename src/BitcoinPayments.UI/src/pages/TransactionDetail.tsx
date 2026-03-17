import { useState } from 'react';
import { useParams, Link } from 'react-router';
import {
  ArrowLeft,
  ExternalLink,
  Copy,
  X,
  AlertTriangle,
  Clock,
  CheckCircle2,
  XCircle,
  Loader2,
  Shield,
  ArrowRight,
  RotateCcw,
} from 'lucide-react';
import {
  useTransaction,
  useTransactionHistory,
  useCaptureAuth,
  useVoidAuth,
  useCreateRefund,
} from '../hooks/usePayments';
import { formatSats, formatDate, shortTxId } from '../lib/formatters';
import StateBadge from '../components/common/StateBadge';
import toast from 'react-hot-toast';

function canCapture(state: string) {
  return state === 'authactive' || state === 'fundingconfirmed';
}
function canVoid(state: string) {
  return state === 'authactive' || state === 'fundingconfirmed';
}
function canRefund(state: string, opType: string) {
  return state === 'settled' && (opType === 'charge' || opType === 'capture' || opType === 'transfer');
}
function isTerminal(state: string) {
  return ['settled', 'voided', 'expired', 'failed', 'transfercompleted'].includes(state);
}
function isInProgress(state: string) {
  return [
    'created', 'broadcasting', 'mempool', 'confirming',
    'fundingbroadcast', 'fundingconfirmed',
    'capturebroadcast', 'captureconfirming',
    'voidbroadcast', 'voidconfirming',
  ].includes(state);
}

function StateIcon({ state }: { state: string }) {
  if (state === 'settled' || state === 'transfercompleted')
    return <CheckCircle2 size={18} className="text-green-400" />;
  if (state === 'voided' || state === 'expired')
    return <XCircle size={18} className="text-zinc-400" />;
  if (state === 'failed')
    return <AlertTriangle size={18} className="text-red-400" />;
  if (isInProgress(state))
    return <Loader2 size={18} className="text-amber-400 animate-spin" />;
  return <Clock size={18} className="text-zinc-500" />;
}

export default function TransactionDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: tx, isLoading } = useTransaction(id!);
  const { data: history } = useTransactionHistory(id!);
  const capture = useCaptureAuth();
  const voidTx = useVoidAuth();
  const refund = useCreateRefund();

  const [modal, setModal] = useState<'capture' | 'void' | 'refund' | null>(null);
  const [amount, setAmount] = useState('');

  if (isLoading)
    return (
      <div className="space-y-4">
        <div className="skeleton h-4 w-32" />
        <div className="glass-card-solid p-6 space-y-4">
          <div className="skeleton h-8 w-48" />
          <div className="skeleton h-4 w-full" />
          <div className="skeleton h-4 w-3/4" />
        </div>
      </div>
    );

  if (!tx)
    return (
      <div className="glass-card-solid p-14 text-center">
        <p className="text-sm text-zinc-400">Transaction not found.</p>
      </div>
    );

  const handleCapture = async () => {
    try {
      await capture.mutateAsync({
        authorizationId: tx.id,
        amountSatoshis: parseInt(amount) || tx.amountSatoshis,
      });
      toast.success('Capture submitted');
      setModal(null);
      setAmount('');
    } catch (err: any) {
      toast.error(err.response?.data?.error?.message || 'Capture failed');
    }
  };

  const handleVoid = async () => {
    try {
      await voidTx.mutateAsync({ authorizationId: tx.id });
      toast.success('Void submitted');
      setModal(null);
    } catch (err: any) {
      toast.error(err.response?.data?.error?.message || 'Void failed');
    }
  };

  const handleRefund = async () => {
    try {
      await refund.mutateAsync({
        parentTransactionId: tx.id,
        amountSatoshis: parseInt(amount) || tx.amountSatoshis,
      });
      toast.success('Refund submitted');
      setModal(null);
      setAmount('');
    } catch (err: any) {
      toast.error(err.response?.data?.error?.message || 'Refund failed');
    }
  };

  const copyTxId = () => {
    if (tx.bitcoinTxId) {
      navigator.clipboard.writeText(tx.bitcoinTxId);
      toast.success('Transaction ID copied');
    }
  };

  const state = tx.state?.toLowerCase();
  const opType = tx.operationType?.toLowerCase();

  return (
    <div>
      {/* Back link */}
      <Link
        to="/transactions"
        className="inline-flex items-center gap-2 text-sm text-zinc-500 hover:text-zinc-200 transition-colors mb-6 animate-fade-up"
        style={{ fontFamily: 'var(--font-body)' }}
      >
        <ArrowLeft size={16} />
        Back to Transactions
      </Link>

      {/* Header */}
      <div className="flex items-start justify-between mb-6 animate-fade-up delay-1">
        <div>
          <div className="flex items-center gap-3 mb-2">
            <StateIcon state={state} />
            <h2
              className="text-2xl font-bold text-zinc-100 tracking-tight capitalize"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              {opType}
            </h2>
            <StateBadge state={state} />
          </div>
          <p className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
            Created {formatDate(tx.createdAt)}
          </p>
        </div>
        <div
          className="text-2xl font-bold text-zinc-100"
          style={{ fontFamily: 'var(--font-mono)' }}
        >
          {formatSats(tx.amountSatoshis)}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        {/* Details card */}
        <div className="lg:col-span-2 glass-card-solid p-6 animate-fade-up delay-2">
          <h3
            className="text-sm font-semibold text-zinc-200 mb-5"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Transaction Details
          </h3>

          <div className="space-y-3">
            {[
              { label: 'Transaction ID', value: tx.id, mono: true },
              { label: 'Operation', value: opType, mono: false, capitalize: true },
              { label: 'State', value: state, mono: false, badge: true },
              { label: 'Amount', value: formatSats(tx.amountSatoshis), mono: true },
              {
                label: 'Fee',
                value: tx.feeSatoshis != null ? formatSats(tx.feeSatoshis) : 'Pending',
                mono: true,
              },
              { label: 'Confirmations', value: tx.confirmationCount.toString(), mono: true },
              { label: 'Created', value: formatDate(tx.createdAt), mono: false },
              { label: 'Updated', value: formatDate(tx.updatedAt), mono: false },
            ].map(({ label, value, mono, badge, capitalize: cap }) => (
              <div
                key={label}
                className="flex items-center justify-between py-2 border-b border-zinc-800/40 last:border-0"
              >
                <span className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
                  {label}
                </span>
                {badge ? (
                  <StateBadge state={value} />
                ) : (
                  <span
                    className={`text-sm font-medium text-zinc-200 ${cap ? 'capitalize' : ''}`}
                    style={{ fontFamily: mono ? 'var(--font-mono)' : 'var(--font-body)' }}
                  >
                    {value}
                  </span>
                )}
              </div>
            ))}

            {/* Bitcoin Tx ID */}
            {tx.bitcoinTxId && (
              <div className="flex items-center justify-between py-2">
                <span className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
                  Bitcoin Tx
                </span>
                <div className="flex items-center gap-2">
                  <span
                    className="text-sm text-zinc-200"
                    style={{ fontFamily: 'var(--font-mono)' }}
                  >
                    {shortTxId(tx.bitcoinTxId)}
                  </span>
                  <button
                    onClick={copyTxId}
                    className="p-1 rounded hover:bg-zinc-800/50 text-zinc-500 hover:text-amber-400 transition-colors"
                  >
                    <Copy size={12} />
                  </button>
                  {tx.explorerUrl && (
                    <a
                      href={tx.explorerUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="p-1 rounded hover:bg-zinc-800/50 text-amber-400 hover:text-amber-300 transition-colors"
                    >
                      <ExternalLink size={12} />
                    </a>
                  )}
                </div>
              </div>
            )}

            {/* Error message */}
            {tx.errorMessage && (
              <div className="flex items-center gap-2 px-3 py-2 bg-red-500/8 border border-red-500/15 rounded-xl mt-2">
                <AlertTriangle size={14} className="text-red-400 shrink-0" />
                <span className="text-xs text-red-400" style={{ fontFamily: 'var(--font-body)' }}>
                  {tx.errorMessage}
                </span>
              </div>
            )}
          </div>
        </div>

        {/* Actions card */}
        <div className="glass-card-solid p-6 animate-fade-up delay-3">
          <h3
            className="text-sm font-semibold text-zinc-200 mb-5"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Actions
          </h3>

          {isInProgress(state) && (
            <div className="flex items-center gap-2 px-3 py-2 bg-amber-500/8 border border-amber-500/15 rounded-xl mb-4">
              <Loader2 size={14} className="text-amber-400 animate-spin" />
              <span className="text-xs text-amber-400" style={{ fontFamily: 'var(--font-body)' }}>
                Transaction in progress...
              </span>
            </div>
          )}

          <div className="space-y-2.5">
            {canCapture(state) && (
              <button
                onClick={() => { setAmount(tx.amountSatoshis.toString()); setModal('capture'); }}
                className="w-full flex items-center gap-2 px-4 py-2.5 bg-green-500/10 hover:bg-green-500/15 text-green-400 border border-green-500/20 rounded-xl text-sm font-medium transition-all"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                <ArrowRight size={15} />
                Capture
              </button>
            )}

            {canVoid(state) && (
              <button
                onClick={() => setModal('void')}
                className="w-full flex items-center gap-2 px-4 py-2.5 bg-zinc-500/10 hover:bg-zinc-500/15 text-zinc-300 border border-zinc-700/50 rounded-xl text-sm font-medium transition-all"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                <XCircle size={15} />
                Void
              </button>
            )}

            {canRefund(state, opType) && (
              <button
                onClick={() => { setAmount(tx.amountSatoshis.toString()); setModal('refund'); }}
                className="w-full flex items-center gap-2 px-4 py-2.5 bg-amber-500/10 hover:bg-amber-500/15 text-amber-400 border border-amber-500/20 rounded-xl text-sm font-medium transition-all"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                <RotateCcw size={15} />
                Refund
              </button>
            )}

            {isTerminal(state) && !canRefund(state, opType) && (
              <p className="text-xs text-zinc-600 text-center py-4" style={{ fontFamily: 'var(--font-body)' }}>
                No actions available — transaction is {state}
              </p>
            )}

            {!isTerminal(state) && !canCapture(state) && !canVoid(state) && (
              <p className="text-xs text-zinc-600 text-center py-4" style={{ fontFamily: 'var(--font-body)' }}>
                Actions will be available once the transaction settles
              </p>
            )}
          </div>
        </div>
      </div>

      {/* History timeline */}
      {history && history.length > 1 && (
        <div className="glass-card-solid p-6 mt-4 animate-fade-up delay-4">
          <h3
            className="text-sm font-semibold text-zinc-200 mb-5"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            <span className="inline-flex items-center gap-2">
              <Shield size={14} className="text-amber-400" />
              Transaction History
            </span>
          </h3>

          <div className="space-y-0">
            {history.map((h, i) => (
              <div key={h.id} className="flex items-start gap-3 relative">
                {/* Timeline line */}
                {i < history.length - 1 && (
                  <div className="absolute left-[11px] top-7 bottom-0 w-px bg-zinc-800/60" />
                )}
                {/* Dot */}
                <div className="mt-1.5 shrink-0">
                  <StateIcon state={h.state?.toLowerCase()} />
                </div>
                {/* Content */}
                <div className={`flex-1 pb-4 ${i < history.length - 1 ? 'border-b border-zinc-800/30' : ''} mb-2`}>
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <span
                        className="text-sm font-medium text-zinc-200 capitalize"
                        style={{ fontFamily: 'var(--font-body)' }}
                      >
                        {h.operationType?.toLowerCase()}
                      </span>
                      <StateBadge state={h.state?.toLowerCase()} />
                    </div>
                    <span
                      className="text-sm text-zinc-300"
                      style={{ fontFamily: 'var(--font-mono)' }}
                    >
                      {formatSats(h.amountSatoshis)}
                    </span>
                  </div>
                  <p className="text-xs text-zinc-600 mt-1" style={{ fontFamily: 'var(--font-body)' }}>
                    {formatDate(h.createdAt)}
                    {h.id !== tx.id && (
                      <Link
                        to={`/transactions/${h.id}`}
                        className="ml-2 text-amber-400 hover:text-amber-300"
                      >
                        View
                      </Link>
                    )}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* ——— Modals ——— */}
      {modal && (
        <div
          className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 animate-fade-in"
          style={{ backdropFilter: 'blur(4px)' }}
          onClick={() => setModal(null)}
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
                {modal === 'capture' && 'Capture Authorization'}
                {modal === 'void' && 'Void Authorization'}
                {modal === 'refund' && 'Refund Transaction'}
              </h3>
              <button
                onClick={() => setModal(null)}
                className="p-1.5 rounded-lg text-zinc-500 hover:text-zinc-300 hover:bg-zinc-800/50 transition-colors"
              >
                <X size={16} />
              </button>
            </div>

            {/* Amount input (capture & refund only) */}
            {(modal === 'capture' || modal === 'refund') && (
              <div className="mb-5">
                <label className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase">
                  Amount (satoshis)
                </label>
                <input
                  type="number"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  min="1"
                  max={tx.amountSatoshis}
                  className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all"
                  style={{ fontFamily: 'var(--font-mono)' }}
                />
                <p className="text-xs text-zinc-600 mt-1.5">
                  Max: {formatSats(tx.amountSatoshis)}
                </p>
              </div>
            )}

            {/* Void confirmation */}
            {modal === 'void' && (
              <div className="flex items-center gap-2 px-3 py-2 bg-amber-500/8 border border-amber-500/15 rounded-xl mb-5">
                <AlertTriangle size={14} className="text-amber-400 shrink-0" />
                <span className="text-xs text-amber-400">
                  This will return {formatSats(tx.amountSatoshis)} to the buyer wallet.
                </span>
              </div>
            )}

            {/* Summary */}
            <div className="space-y-2 mb-5">
              <div className="flex justify-between items-center py-2 border-b border-zinc-800/50">
                <span className="text-sm text-zinc-500">Transaction</span>
                <span className="text-xs text-zinc-300" style={{ fontFamily: 'var(--font-mono)' }}>
                  {tx.id.slice(0, 8)}...
                </span>
              </div>
              <div className="flex justify-between items-center py-2 border-b border-zinc-800/50">
                <span className="text-sm text-zinc-500">Action</span>
                <span className="text-sm text-zinc-200 capitalize font-medium">{modal}</span>
              </div>
              {(modal === 'capture' || modal === 'refund') && (
                <div className="flex justify-between items-center py-2">
                  <span className="text-sm text-zinc-500">Amount</span>
                  <span className="text-sm text-zinc-200" style={{ fontFamily: 'var(--font-mono)' }}>
                    {formatSats(parseInt(amount) || 0)}
                  </span>
                </div>
              )}
            </div>

            {/* Buttons */}
            <div className="flex gap-3">
              <button
                onClick={() => setModal(null)}
                className="flex-1 py-2.5 bg-zinc-800/50 hover:bg-zinc-800 text-zinc-300 rounded-xl text-sm font-medium transition-colors"
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Cancel
              </button>
              <button
                onClick={modal === 'capture' ? handleCapture : modal === 'void' ? handleVoid : handleRefund}
                disabled={capture.isPending || voidTx.isPending || refund.isPending}
                className={`flex-1 py-2.5 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 btn-glow ${
                  modal === 'void'
                    ? 'bg-red-500 hover:bg-red-400 text-white'
                    : 'bg-amber-500 hover:bg-amber-400 text-zinc-950'
                }`}
                style={{ fontFamily: 'var(--font-display)' }}
              >
                {(capture.isPending || voidTx.isPending || refund.isPending) ? (
                  <span className="inline-flex items-center gap-2">
                    <span className="w-3.5 h-3.5 border-2 border-current/30 border-t-current rounded-full animate-spin" />
                    Processing...
                  </span>
                ) : (
                  `Confirm ${modal.charAt(0).toUpperCase() + modal.slice(1)}`
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
