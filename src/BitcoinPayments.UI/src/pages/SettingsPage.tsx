import { useAuth } from '../hooks/useAuth';
import { Wifi } from 'lucide-react';

export default function SettingsPage() {
  const { user } = useAuth();

  return (
    <div className="max-w-lg">
      <h2 className="text-2xl font-semibold text-zinc-100 mb-6">Settings</h2>

      <div className="bg-zinc-900 border border-zinc-800 rounded-xl p-6 space-y-6">
        <div>
          <h3 className="text-sm font-medium text-zinc-300 mb-3">Account</h3>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-zinc-400">Email</span>
              <span className="text-zinc-200">{user?.email}</span>
            </div>
            <div className="flex justify-between">
              <span className="text-zinc-400">Display Name</span>
              <span className="text-zinc-200">{user?.displayName || '-'}</span>
            </div>
          </div>
        </div>

        <hr className="border-zinc-800" />

        <div>
          <h3 className="text-sm font-medium text-zinc-300 mb-3">Network</h3>
          <div className="flex items-center gap-2">
            <Wifi size={16} className="text-green-400" />
            <span className="text-sm text-zinc-200">testnet4</span>
            <span className="text-xs text-green-400">(connected)</span>
          </div>
        </div>

        <hr className="border-zinc-800" />

        <div>
          <h3 className="text-sm font-medium text-zinc-300 mb-3">Transfer Mode</h3>
          <p className="text-sm text-zinc-400">
            <span className="text-amber-400 font-medium">Auto</span> &mdash; Internal transfers between your wallets, on-chain for external.
          </p>
        </div>
      </div>
    </div>
  );
}
