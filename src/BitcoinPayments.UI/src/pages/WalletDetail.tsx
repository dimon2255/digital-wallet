import { useParams, Link } from 'react-router';
import { QRCodeSVG } from 'qrcode.react';
import { ArrowLeft, Copy, Send, Wallet, Hash } from 'lucide-react';
import { useWallet, useWalletAddress } from '../hooks/useWallets';
import { formatSats } from '../lib/formatters';
import toast from 'react-hot-toast';

export default function WalletDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: wallet, isLoading } = useWallet(id!);
  const { data: addrData } = useWalletAddress(id!);

  if (isLoading)
    return (
      <div className="space-y-4">
        <div className="skeleton h-4 w-32" />
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="glass-card-solid p-6">
            <div className="skeleton h-6 w-40 mb-3" />
            <div className="skeleton h-9 w-52 mb-4" />
            <div className="skeleton h-4 w-full" />
          </div>
          <div className="glass-card-solid p-6">
            <div className="skeleton h-4 w-28 mb-4" />
            <div className="skeleton h-44 w-44 mx-auto rounded-xl" />
          </div>
        </div>
      </div>
    );

  if (!wallet)
    return (
      <div className="glass-card-solid p-14 text-center">
        <p className="text-sm text-zinc-400">Wallet not found.</p>
      </div>
    );

  const copyAddress = () => {
    if (addrData?.address) {
      navigator.clipboard.writeText(addrData.address);
      toast.success('Address copied');
    }
  };

  return (
    <div>
      {/* Back link */}
      <Link
        to="/wallets"
        className="inline-flex items-center gap-2 text-sm text-zinc-500 hover:text-zinc-200 transition-colors mb-6 animate-fade-up"
        style={{ fontFamily: 'var(--font-body)' }}
      >
        <ArrowLeft size={16} />
        Back to Wallets
      </Link>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Wallet Info */}
        <div className="glass-card-solid p-6 animate-fade-up delay-1">
          <div className="flex items-center gap-3 mb-5">
            <div className="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/15 flex items-center justify-center">
              <Wallet size={18} className="text-amber-400" />
            </div>
            <div>
              <h2
                className="text-lg font-bold text-zinc-100 tracking-tight"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                {wallet.name}
              </h2>
              <span className="text-xs text-zinc-600 px-1.5 py-0.5 rounded bg-zinc-800/60">
                {wallet.network}
              </span>
            </div>
          </div>

          {/* Balance */}
          <div className="mb-6">
            <span
              className="text-xs font-medium text-zinc-500 uppercase tracking-wide block mb-1"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              Balance
            </span>
            <div
              className="text-3xl font-bold text-zinc-100"
              style={{ fontFamily: 'var(--font-mono)' }}
            >
              {formatSats(wallet.balanceSatoshis)}
            </div>
          </div>

          {/* Derivation info */}
          <div className="h-px bg-gradient-to-r from-zinc-800/40 via-zinc-800 to-zinc-800/40 mb-5" />
          <div className="grid grid-cols-2 gap-4">
            <div>
              <span
                className="text-xs text-zinc-500 block mb-1"
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Receiving Addresses
              </span>
              <div className="flex items-center gap-1.5">
                <Hash size={13} className="text-zinc-600" />
                <span
                  className="text-sm font-medium text-zinc-200"
                  style={{ fontFamily: 'var(--font-mono)' }}
                >
                  {wallet.currentReceivingIndex}
                </span>
              </div>
            </div>
            <div>
              <span
                className="text-xs text-zinc-500 block mb-1"
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Change Addresses
              </span>
              <div className="flex items-center gap-1.5">
                <Hash size={13} className="text-zinc-600" />
                <span
                  className="text-sm font-medium text-zinc-200"
                  style={{ fontFamily: 'var(--font-mono)' }}
                >
                  {wallet.currentChangeIndex}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Receive Address + QR */}
        <div className="glass-card-solid p-6 animate-fade-up delay-2">
          <h3
            className="text-sm font-semibold text-zinc-200 mb-5"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Receive Address
          </h3>
          {addrData ? (
            <div className="text-center">
              {/* QR Code */}
              <div className="inline-block p-4 bg-white rounded-2xl mb-5 glow-amber-sm">
                <QRCodeSVG value={`bitcoin:${addrData.address}`} size={168} />
              </div>

              {/* Address */}
              <div className="flex items-center gap-2 bg-zinc-950/60 border border-zinc-800 rounded-xl p-3">
                <code
                  className="text-xs text-zinc-300 flex-1 break-all"
                  style={{ fontFamily: 'var(--font-mono)' }}
                >
                  {addrData.address}
                </code>
                <button
                  onClick={copyAddress}
                  className="p-2 rounded-lg hover:bg-zinc-800/60 text-zinc-500 hover:text-amber-400 transition-all duration-200 shrink-0"
                  title="Copy address"
                >
                  <Copy size={14} />
                </button>
              </div>

              <p
                className="text-xs text-zinc-600 mt-3"
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Derivation index: {addrData.derivationIndex}
              </p>
            </div>
          ) : (
            <div className="flex flex-col items-center py-10">
              <div className="skeleton h-44 w-44 rounded-2xl mb-4" />
              <div className="skeleton h-10 w-full rounded-xl" />
            </div>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="mt-6 animate-fade-up delay-3">
        <Link
          to={`/send?from=${id}`}
          className="inline-flex items-center gap-2 px-5 py-2.5 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 btn-glow"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          <Send size={15} />
          Send from this wallet
        </Link>
      </div>
    </div>
  );
}
