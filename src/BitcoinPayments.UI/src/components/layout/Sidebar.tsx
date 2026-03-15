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

  return (
    <aside className="w-64 bg-zinc-900 border-r border-zinc-800 flex flex-col h-screen fixed">
      <div className="p-6">
        <h1 className="text-xl font-bold text-amber-500 tracking-tight">ChainVault</h1>
        <p className="text-xs text-zinc-500 mt-1">Bitcoin Payment Platform</p>
      </div>

      <nav className="flex-1 px-3">
        {links.map(({ to, label, icon: Icon }) => {
          const active = location.pathname === to || (to !== '/' && location.pathname.startsWith(to));
          return (
            <Link
              key={to}
              to={to}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg mb-1 text-sm transition-colors ${
                active
                  ? 'bg-amber-500/10 text-amber-400'
                  : 'text-zinc-400 hover:text-zinc-200 hover:bg-zinc-800'
              }`}
            >
              <Icon size={18} />
              {label}
            </Link>
          );
        })}
      </nav>

      <div className="p-4 border-t border-zinc-800">
        <div className="text-sm text-zinc-400 truncate mb-2">{user?.displayName || user?.email}</div>
        <button
          onClick={logout}
          className="flex items-center gap-2 text-xs text-zinc-500 hover:text-red-400 transition-colors"
        >
          <LogOut size={14} />
          Sign out
        </button>
      </div>
    </aside>
  );
}
