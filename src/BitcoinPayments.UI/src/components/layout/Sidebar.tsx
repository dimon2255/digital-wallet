import { Link, useLocation } from 'react-router-dom';
import { LayoutDashboard, Wallet, ArrowLeftRight, History, Settings, LogOut } from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';

const links = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/wallets', label: 'Wallets', icon: Wallet },
  { to: '/send', label: 'Send', icon: ArrowLeftRight },
  { to: '/transactions', label: 'History', icon: History },
  { to: '/settings', label: 'Settings', icon: Settings },
];

export default function Sidebar() {
  const location = useLocation();
  const { user, logout } = useAuth();

  const initials = (user?.displayName || user?.email || '?')
    .split(/[\s@]/)
    .slice(0, 2)
    .map((s) => s[0]?.toUpperCase())
    .join('');

  return (
    <aside className="w-64 flex flex-col h-screen fixed z-30 border-r border-zinc-800/70 bg-zinc-950">
      {/* Subtle sidebar gradient */}
      <div
        className="absolute inset-0 pointer-events-none"
        style={{
          background:
            'linear-gradient(180deg, rgba(24,24,27,0.5) 0%, rgba(9,9,11,1) 100%)',
        }}
      />

      {/* Brand */}
      <div className="relative px-6 pt-6 pb-5">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center shrink-0">
            <span
              className="text-sm font-bold text-amber-400"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              ₿
            </span>
          </div>
          <div>
            <h1
              className="text-base font-bold text-zinc-100 tracking-tight leading-none"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Chain<span className="text-amber-400">Vault</span>
            </h1>
            <p
              className="text-[10px] text-zinc-600 mt-0.5 tracking-wide uppercase"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              Payment Platform
            </p>
          </div>
        </div>
      </div>

      {/* Divider */}
      <div className="mx-4 h-px bg-gradient-to-r from-zinc-800/80 via-zinc-800 to-zinc-800/80" />

      {/* Navigation */}
      <nav className="relative flex-1 px-3 pt-4 space-y-0.5">
        {links.map(({ to, label, icon: Icon }) => {
          const active = location.pathname === to || (to !== '/' && location.pathname.startsWith(to));
          return (
            <Link
              key={to}
              to={to}
              className={`group relative flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-200 ${
                active
                  ? 'text-amber-400 bg-amber-500/8'
                  : 'text-zinc-500 hover:text-zinc-200 hover:bg-zinc-800/50'
              }`}
              style={{ fontFamily: 'var(--font-body)' }}
            >
              {/* Active indicator bar */}
              {active && (
                <div className="absolute left-0 top-1/2 -translate-y-1/2 w-[3px] h-5 rounded-r-full bg-amber-400" />
              )}
              <Icon
                size={18}
                className={`shrink-0 transition-colors duration-200 ${
                  active ? 'text-amber-400' : 'text-zinc-600 group-hover:text-zinc-400'
                }`}
              />
              {label}
            </Link>
          );
        })}
      </nav>

      {/* User section */}
      <div className="relative px-4 pb-4 pt-3">
        <div className="mx-0 mb-3 h-px bg-gradient-to-r from-zinc-800/80 via-zinc-800 to-zinc-800/80" />
        <div className="flex items-center gap-3">
          {/* Avatar */}
          <div className="w-8 h-8 rounded-lg bg-zinc-800 border border-zinc-700/50 flex items-center justify-center shrink-0">
            <span
              className="text-xs font-semibold text-zinc-400"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              {initials}
            </span>
          </div>
          <div className="flex-1 min-w-0">
            <div
              className="text-sm font-medium text-zinc-300 truncate"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              {user?.displayName || 'User'}
            </div>
            <div className="text-xs text-zinc-600 truncate">
              {user?.email}
            </div>
          </div>
          <button
            onClick={logout}
            className="p-1.5 rounded-lg text-zinc-600 hover:text-red-400 hover:bg-zinc-800/50 transition-all duration-200"
            title="Sign out"
          >
            <LogOut size={15} />
          </button>
        </div>
      </div>
    </aside>
  );
}
