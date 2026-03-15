import { useParams, Link } from 'react-router-dom';
import { QRCodeSVG } from 'qrcode.react';
import { ArrowLeft, Copy } from 'lucide-react';
import { useWallet, useWalletAddress } from '../hooks/useWallets';
import { formatSats } from '../lib/formatters';
import toast from 'react-hot-toast';

export default function WalletDetail() {
  const { id } = useParams<{ id: string }>();
  const { data: wallet, isLoading } = useWallet(id!);
  const { data: addrData } = useWalletAddress(id!);

  if (isLoading) return <p className="text-zinc-500">Loading...</p>;
  if (!wallet) return <p className="text-zinc-400">Wallet not found.</p>;

  const copyAddress = () => {
    if (addrData?.address) {
      navigator.clipboard.writeText(addrData.address);
      toast.success('Address copied');
    }
  };

  return (
    <div>
      <Link to="/wallets" className="flex items-center gap-2 text-sm text-zinc-400 hover:text-zinc-200 mb-6">
        <ArrowLeft size={16} />
        Back to Wallets
      </Link>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-6">
          <h2 className="text-xl font-semibold text-zinc-100 mb-1">{wallet.name}</h2>
          <p className="text-xs text-zinc-500 mb-4">{wallet.network}</p>
          <div className="text-3xl font-bold text-zinc-100 font-mono mb-4">{formatSats(wallet.balanceSatoshis)}</div>
          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-zinc-500">Receiving Addresses</span>
              <p className="text-zinc-200 font-mono">{wallet.currentReceivingIndex}</p>
            </div>
            <div>
              <span className="text-zinc-500">Change Addresses</span>
              <p className="text-zinc-200 font-mono">{wallet.currentChangeIndex}</p>
            </div>
          </div>
        </div>

        <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-6">
          <h3 className="text-sm font-medium text-zinc-300 mb-4">Receive Address</h3>
          {addrData ? (
            <div className="text-center">
              <div className="inline-block p-3 bg-white rounded-lg mb-4">
                <QRCodeSVG value={`bitcoin:${addrData.address}`} size={160} />
              </div>
              <div className="flex items-center gap-2 bg-zinc-800 rounded-lg p-3">
                <code className="text-xs text-zinc-300 flex-1 break-all">{addrData.address}</code>
                <button onClick={copyAddress} className="p-1.5 hover:bg-zinc-700 rounded transition-colors">
                  <Copy size={14} className="text-zinc-400" />
                </button>
              </div>
              <p className="text-xs text-zinc-500 mt-2">Derivation index: {addrData.derivationIndex}</p>
            </div>
          ) : (
            <p className="text-zinc-500 text-sm">Loading address...</p>
          )}
        </div>
      </div>

      <div className="mt-6 flex gap-3">
        <Link
          to={`/send?from=${id}`}
          className="px-4 py-2 bg-amber-500 hover:bg-amber-600 text-zinc-900 font-medium rounded-lg text-sm transition-colors"
        >
          Send from this wallet
        </Link>
      </div>
    </div>
  );
}
