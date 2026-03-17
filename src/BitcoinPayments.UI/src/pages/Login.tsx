import { useState, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';
import { login, register, demoLogin } from '../api/auth';
import toast from 'react-hot-toast';
import { Shield, ArrowRight, Zap, Lock, ChevronDown, ArrowLeftRight, Layers } from 'lucide-react';

export default function Login() {
  const [isRegister, setIsRegister] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const auth = useAuth();
  const formRef = useRef<HTMLDivElement>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const res = isRegister
        ? await register(email, password, displayName || undefined)
        : await login(email, password);
      auth.login(res.accessToken, res.refreshToken, res.user);
      navigate('/');
    } catch (err: any) {
      toast.error(err.response?.data?.error?.message || err.response?.data?.errors?.[0] || 'Authentication failed');
    } finally {
      setLoading(false);
    }
  };

  const handleDemo = async () => {
    setLoading(true);
    try {
      const res = await demoLogin();
      auth.login(res.accessToken, res.refreshToken, res.user);
      navigate('/');
    } catch {
      toast.error('Demo login failed');
    } finally {
      setLoading(false);
    }
  };

  const scrollToForm = useCallback((registerMode?: boolean) => {
    if (registerMode !== undefined) setIsRegister(registerMode);
    formRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, []);

  return (
    <div className="bg-zinc-950 relative">
      {/* Noise */}
      <div className="noise-overlay" />

      {/* ====== Navbar ====== */}
      <nav className="fixed top-0 left-0 right-0 z-40 animate-fade-in">
        <div
          className="mx-auto flex items-center justify-between px-6 sm:px-10 py-4"
          style={{
            background: 'rgba(9,9,11,0.7)',
            backdropFilter: 'blur(12px)',
            WebkitBackdropFilter: 'blur(12px)',
          }}
        >
          {/* Logo */}
          <span
            className="text-lg font-bold text-zinc-100 tracking-tight cursor-default"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Chain<span className="text-amber-400">Vault</span>
          </span>

          {/* Nav actions */}
          <div className="flex items-center gap-2">
            <button
              onClick={() => scrollToForm(false)}
              className="px-4 py-2 text-sm font-medium text-zinc-400 hover:text-zinc-100 rounded-lg transition-colors"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              Log In
            </button>
            <button
              onClick={() => scrollToForm(true)}
              className="px-4 py-2 text-sm font-medium text-zinc-950 bg-amber-500 hover:bg-amber-400 rounded-lg transition-all duration-200 btn-glow"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Register
            </button>
          </div>
        </div>
        {/* Bottom border */}
        <div className="h-px bg-gradient-to-r from-transparent via-zinc-800/60 to-transparent" />
      </nav>

      {/* ====== SECTION 1: Hero ====== */}
      <section className="min-h-screen relative flex flex-col items-center justify-center px-6 overflow-hidden">
        {/* Background atmosphere */}
        <div className="absolute inset-0 bg-vault" />
        <div
          className="absolute inset-0"
          style={{
            background:
              'radial-gradient(ellipse 50% 40% at 50% 40%, rgba(245,158,11,0.07) 0%, transparent 70%)',
          }}
        />

        {/* Grid pattern */}
        <div
          className="absolute inset-0 opacity-[0.03]"
          style={{
            backgroundImage:
              'linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)',
            backgroundSize: '72px 72px',
          }}
        />

        {/* Content */}
        <div className="relative z-10 text-center max-w-2xl">
          {/* Badge */}
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full border border-zinc-800 bg-zinc-900/60 mb-8 animate-fade-up">
            <div className="w-2 h-2 rounded-full bg-green-400 animate-pulse" />
            <span className="text-xs text-zinc-400 tracking-wide" style={{ fontFamily: 'var(--font-body)' }}>
              Testnet4 — Live Environment
            </span>
          </div>

          {/* Bitcoin symbol */}
          <div className="flex justify-center mb-8 animate-fade-up delay-1">
            <div className="w-16 h-16 rounded-2xl bg-amber-500/10 border border-amber-500/20 flex items-center justify-center animate-pulse-glow">
              <span className="text-2xl font-bold text-amber-400" style={{ fontFamily: 'var(--font-display)' }}>₿</span>
            </div>
          </div>

          {/* Headline */}
          <h1
            className="text-5xl sm:text-6xl md:text-7xl font-extrabold tracking-tight text-zinc-100 leading-[1.05] animate-fade-up delay-2"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Chain<span className="text-amber-400">Vault</span>
          </h1>

          <p
            className="mt-5 text-lg sm:text-xl text-zinc-400 leading-relaxed max-w-lg mx-auto animate-fade-up delay-3"
            style={{ fontFamily: 'var(--font-body)' }}
          >
            Enterprise-grade Bitcoin payment processing.
            <br className="hidden sm:block" />
            Secure. Programmable. Non-custodial.
          </p>

          {/* CTA */}
          <div className="mt-10 flex flex-col sm:flex-row items-center justify-center gap-3 animate-fade-up delay-4">
            <button
              onClick={() => scrollToForm(true)}
              className="px-8 py-3 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 btn-glow"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              <span className="inline-flex items-center gap-2">
                Get Started
                <ArrowRight size={16} />
              </span>
            </button>
            <button
              onClick={handleDemo}
              disabled={loading}
              className="px-8 py-3 bg-transparent hover:bg-zinc-900 text-zinc-300 hover:text-zinc-100 font-medium rounded-xl text-sm transition-all duration-200 border border-zinc-800 hover:border-zinc-700 disabled:opacity-50"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              <span className="inline-flex items-center gap-2">
                <Zap size={14} className="text-amber-400" />
                Try Demo
              </span>
            </button>
          </div>
        </div>

        {/* Scroll indicator */}
        <button
          onClick={() => scrollToForm()}
          className="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 text-zinc-600 hover:text-zinc-400 transition-colors animate-fade-in delay-6 cursor-pointer"
        >
          <span className="text-xs tracking-widest uppercase" style={{ fontFamily: 'var(--font-display)' }}>Scroll</span>
          <ChevronDown size={16} className="animate-bounce" />
        </button>
      </section>

      {/* ====== SECTION 2: Features ====== */}
      <section className="relative py-24 px-6">
        {/* Top fade border */}
        <div className="absolute top-0 left-[10%] right-[10%] h-px bg-gradient-to-r from-transparent via-zinc-800 to-transparent" />

        <div className="max-w-5xl mx-auto">
          <div className="text-center mb-14">
            <h2
              className="text-2xl sm:text-3xl font-bold text-zinc-100 tracking-tight"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Built for serious Bitcoin operations
            </h2>
            <p className="mt-3 text-zinc-500 max-w-md mx-auto" style={{ fontFamily: 'var(--font-body)' }}>
              Full payment lifecycle — from charge to settlement
            </p>
          </div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {[
              {
                icon: Shield,
                title: 'HD Wallet Security',
                desc: 'BIP32 key derivation with DataProtection encryption. User-scoped wallet isolation.',
              },
              {
                icon: ArrowLeftRight,
                title: 'Payment Flows',
                desc: 'Charge, authorize, capture, void, and refund — full payment lifecycle on-chain.',
              },
              {
                icon: Zap,
                title: 'Instant Transfers',
                desc: 'Internal ledger for same-user moves. On-chain settlement for cross-wallet transfers.',
              },
              {
                icon: Lock,
                title: 'Escrow System',
                desc: 'Authorize flow uses on-chain escrow with timelock scripts. Non-custodial by design.',
              },
              {
                icon: Layers,
                title: 'UTXO Management',
                desc: 'Coin selection, fee estimation, and UTXO tracking synced from mempool.space.',
              },
              {
                icon: ArrowRight,
                title: 'REST API',
                desc: 'Idempotent endpoints, JWT auth, paginated queries. Build on top of ChainVault.',
              },
            ].map(({ icon: Icon, title, desc }) => (
              <div
                key={title}
                className="glass-card-solid p-6 group"
              >
                <div className="w-10 h-10 rounded-xl bg-amber-500/10 border border-amber-500/15 flex items-center justify-center mb-4 group-hover:glow-amber-sm transition-all duration-300">
                  <Icon size={18} className="text-amber-400" />
                </div>
                <h3
                  className="text-sm font-semibold text-zinc-200 mb-2"
                  style={{ fontFamily: 'var(--font-display)' }}
                >
                  {title}
                </h3>
                <p className="text-sm text-zinc-500 leading-relaxed" style={{ fontFamily: 'var(--font-body)' }}>
                  {desc}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ====== SECTION 3: Auth Form ====== */}
      <section ref={formRef} className="relative py-24 px-6">
        {/* Top fade border */}
        <div className="absolute top-0 left-[10%] right-[10%] h-px bg-gradient-to-r from-transparent via-zinc-800 to-transparent" />

        {/* Background glow behind form */}
        <div
          className="absolute inset-0 pointer-events-none"
          style={{
            background:
              'radial-gradient(ellipse 40% 50% at 50% 50%, rgba(245,158,11,0.04) 0%, transparent 70%)',
          }}
        />

        <div className="max-w-[420px] mx-auto relative z-10">
          {/* Form heading */}
          <div className="text-center mb-8">
            <h2
              className="text-2xl font-bold text-zinc-100 tracking-tight"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              {isRegister ? 'Create your account' : 'Welcome back'}
            </h2>
            <p className="text-sm text-zinc-500 mt-2" style={{ fontFamily: 'var(--font-body)' }}>
              {isRegister
                ? 'Start processing Bitcoin payments in minutes'
                : 'Sign in to your ChainVault account'}
            </p>
          </div>

          {/* Card */}
          <div className="glass-card-solid p-6 sm:p-8">
            {/* Tab switcher */}
            <div className="flex mb-6 bg-zinc-950/60 rounded-xl p-1 border border-zinc-800/60">
              <button
                onClick={() => setIsRegister(false)}
                className={`flex-1 py-2.5 text-sm rounded-lg font-medium transition-all duration-200 ${
                  !isRegister
                    ? 'bg-zinc-800 text-zinc-100 shadow-sm'
                    : 'text-zinc-500 hover:text-zinc-300'
                }`}
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Sign In
              </button>
              <button
                onClick={() => setIsRegister(true)}
                className={`flex-1 py-2.5 text-sm rounded-lg font-medium transition-all duration-200 ${
                  isRegister
                    ? 'bg-zinc-800 text-zinc-100 shadow-sm'
                    : 'text-zinc-500 hover:text-zinc-300'
                }`}
                style={{ fontFamily: 'var(--font-body)' }}
              >
                Register
              </button>
            </div>

            {/* Form */}
            <form onSubmit={handleSubmit} className="space-y-4">
              {isRegister && (
                <div>
                  <label className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase">
                    Display Name
                  </label>
                  <input
                    type="text"
                    placeholder="Satoshi"
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 placeholder:text-zinc-600 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all"
                    style={{ fontFamily: 'var(--font-body)' }}
                  />
                </div>
              )}
              <div>
                <label className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase">
                  Email
                </label>
                <input
                  type="email"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 placeholder:text-zinc-600 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all"
                  style={{ fontFamily: 'var(--font-body)' }}
                />
              </div>
              <div>
                <label className="block text-xs font-medium text-zinc-400 mb-1.5 tracking-wide uppercase">
                  Password
                </label>
                <input
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  className="w-full px-3.5 py-2.5 bg-zinc-950/60 border border-zinc-800 rounded-xl text-sm text-zinc-200 placeholder:text-zinc-600 focus:outline-none focus:border-amber-500/50 focus:ring-1 focus:ring-amber-500/20 transition-all"
                  style={{ fontFamily: 'var(--font-body)' }}
                />
              </div>

              <div className="pt-2">
                <button
                  type="submit"
                  disabled={loading}
                  className="w-full py-3 bg-amber-500 hover:bg-amber-400 text-zinc-950 font-semibold rounded-xl text-sm transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed btn-glow"
                  style={{ fontFamily: 'var(--font-display)' }}
                >
                  {loading ? (
                    <span className="inline-flex items-center gap-2">
                      <span className="w-4 h-4 border-2 border-zinc-900/30 border-t-zinc-900 rounded-full animate-spin" />
                      Processing...
                    </span>
                  ) : isRegister ? (
                    <span className="inline-flex items-center gap-1.5">
                      Create Account
                      <ArrowRight size={15} />
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1.5">
                      Sign In
                      <ArrowRight size={15} />
                    </span>
                  )}
                </button>
              </div>
            </form>

            {/* Divider */}
            <div className="flex items-center gap-3 my-5">
              <div className="flex-1 h-px bg-zinc-800" />
              <span className="text-xs text-zinc-600 uppercase tracking-wider">or</span>
              <div className="flex-1 h-px bg-zinc-800" />
            </div>

            {/* Demo button */}
            <button
              onClick={handleDemo}
              disabled={loading}
              className="w-full py-2.5 bg-transparent hover:bg-zinc-800/50 text-amber-400 hover:text-amber-300 font-medium rounded-xl text-sm transition-all duration-200 border border-zinc-800 hover:border-zinc-700 disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ fontFamily: 'var(--font-body)' }}
            >
              <span className="inline-flex items-center gap-2">
                <Zap size={14} />
                Try Demo Account
              </span>
            </button>
          </div>

          <p className="text-center text-xs text-zinc-600 mt-5">
            Testnet4 environment — no real funds
          </p>
        </div>
      </section>

      {/* ====== Footer ====== */}
      <footer className="relative py-8 px-6">
        <div className="absolute top-0 left-[10%] right-[10%] h-px bg-gradient-to-r from-transparent via-zinc-800/50 to-transparent" />
        <div className="flex items-center justify-center gap-3">
          <span
            className="text-sm font-semibold text-zinc-600"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Chain<span className="text-zinc-500">Vault</span>
          </span>
          <div className="w-1 h-1 rounded-full bg-zinc-800" />
          <span className="text-xs text-zinc-700" style={{ fontFamily: 'var(--font-body)' }}>
            Bitcoin Payment Platform
          </span>
        </div>
      </footer>
    </div>
  );
}
