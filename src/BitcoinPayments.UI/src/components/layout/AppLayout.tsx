import { Outlet } from 'react-router';
import Sidebar from './Sidebar';

export default function AppLayout() {
  return (
    <div className="flex min-h-screen bg-zinc-950">
      <Sidebar />
      <main className="flex-1 ml-64 relative">
        {/* Background atmosphere */}
        <div className="fixed inset-0 ml-64 pointer-events-none bg-vault" />
        <div className="noise-overlay" style={{ left: '16rem' }} />

        {/* Page content */}
        <div className="relative z-10 p-8">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
