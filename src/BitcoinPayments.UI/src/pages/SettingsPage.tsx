import { useAuth } from '../hooks/useAuth';
import { Wifi, User, Zap, Shield } from 'lucide-react';

export default function SettingsPage() {
  const { user } = useAuth();

  return (
    <div className="max-w-lg">
      {/* Header */}
      <div className="mb-8 animate-fade-up">
        <h2
          className="text-2xl font-bold text-zinc-100 tracking-tight"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Settings
        </h2>
        <p className="text-sm text-zinc-500 mt-1" style={{ fontFamily: 'var(--font-body)' }}>
          Account and network configuration
        </p>
      </div>

      <div className="space-y-4">
        {/* Account */}
        <div className="glass-card-solid p-6 animate-fade-up delay-1">
          <div className="flex items-center gap-3 mb-5">
            <div className="w-9 h-9 rounded-xl bg-amber-500/10 border border-amber-500/15 flex items-center justify-center">
              <User size={16} className="text-amber-400" />
            </div>
            <h3
              className="text-sm font-semibold text-zinc-200"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Account
            </h3>
          </div>
          <div className="space-y-3">
            {[
              { label: 'Email', value: user?.email || '-' },
              { label: 'Display Name', value: user?.displayName || '-' },
            ].map(({ label, value }) => (
              <div key={label} className="flex items-center justify-between py-2 border-b border-zinc-800/40 last:border-0">
                <span className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
                  {label}
                </span>
                <span className="text-sm font-medium text-zinc-200" style={{ fontFamily: 'var(--font-body)' }}>
                  {value}
                </span>
              </div>
            ))}
          </div>
        </div>

        {/* Network */}
        <div className="glass-card-solid p-6 animate-fade-up delay-2">
          <div className="flex items-center gap-3 mb-5">
            <div className="w-9 h-9 rounded-xl bg-green-500/10 border border-green-500/15 flex items-center justify-center">
              <Wifi size={16} className="text-green-400" />
            </div>
            <h3
              className="text-sm font-semibold text-zinc-200"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Network
            </h3>
          </div>
          <div className="flex items-center gap-3">
            <span
              className="text-sm font-medium text-zinc-200"
              style={{ fontFamily: 'var(--font-mono)' }}
            >
              testnet4
            </span>
            <span className="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-lg bg-green-500/10 text-xs font-medium text-green-400">
              <span className="w-1.5 h-1.5 rounded-full bg-green-400 animate-pulse" />
              connected
            </span>
          </div>
        </div>

        {/* Transfer Mode */}
        <div className="glass-card-solid p-6 animate-fade-up delay-3">
          <div className="flex items-center gap-3 mb-5">
            <div className="w-9 h-9 rounded-xl bg-amber-500/10 border border-amber-500/15 flex items-center justify-center">
              <Zap size={16} className="text-amber-400" />
            </div>
            <h3
              className="text-sm font-semibold text-zinc-200"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Transfer Mode
            </h3>
          </div>
          <div className="flex items-center gap-2 mb-2">
            <span
              className="text-sm font-semibold text-amber-400"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Auto
            </span>
          </div>
          <p className="text-sm text-zinc-500 leading-relaxed" style={{ fontFamily: 'var(--font-body)' }}>
            Internal transfers between your wallets, on-chain for external.
          </p>
        </div>

        {/* Security */}
        <div className="glass-card-solid p-6 animate-fade-up delay-4">
          <div className="flex items-center gap-3 mb-5">
            <div className="w-9 h-9 rounded-xl bg-blue-500/10 border border-blue-500/15 flex items-center justify-center">
              <Shield size={16} className="text-blue-400" />
            </div>
            <h3
              className="text-sm font-semibold text-zinc-200"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Security
            </h3>
          </div>
          <div className="space-y-3">
            {[
              { label: 'Authentication', value: 'JWT Bearer' },
              { label: 'Key Derivation', value: 'BIP32 HD Wallets' },
              { label: 'Encryption', value: 'ASP.NET DataProtection' },
            ].map(({ label, value }) => (
              <div key={label} className="flex items-center justify-between py-2 border-b border-zinc-800/40 last:border-0">
                <span className="text-sm text-zinc-500" style={{ fontFamily: 'var(--font-body)' }}>
                  {label}
                </span>
                <span
                  className="text-xs font-medium text-zinc-300 px-2 py-0.5 rounded-md bg-zinc-800/60"
                  style={{ fontFamily: 'var(--font-mono)' }}
                >
                  {value}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
