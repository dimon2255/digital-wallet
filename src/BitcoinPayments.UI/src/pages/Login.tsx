import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { login, register, demoLogin } from '../api/auth';
import toast from 'react-hot-toast';

export default function Login() {
  const [isRegister, setIsRegister] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const auth = useAuth();

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

  return (
    <div className="min-h-screen flex items-center justify-center bg-zinc-950 p-4">
      <div className="w-full max-w-md">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-amber-500 tracking-tight">ChainVault</h1>
          <p className="text-zinc-500 mt-2">Bitcoin Payment Platform</p>
        </div>

        <div className="bg-zinc-900 rounded-xl border border-zinc-800 p-6">
          <div className="flex mb-6 bg-zinc-800 rounded-lg p-1">
            <button
              onClick={() => setIsRegister(false)}
              className={`flex-1 py-2 text-sm rounded-md transition-colors ${
                !isRegister ? 'bg-zinc-700 text-white' : 'text-zinc-400'
              }`}
            >
              Sign In
            </button>
            <button
              onClick={() => setIsRegister(true)}
              className={`flex-1 py-2 text-sm rounded-md transition-colors ${
                isRegister ? 'bg-zinc-700 text-white' : 'text-zinc-400'
              }`}
            >
              Register
            </button>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            {isRegister && (
              <input
                type="text"
                placeholder="Display Name"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 placeholder:text-zinc-500 focus:outline-none focus:border-amber-500"
              />
            )}
            <input
              type="email"
              placeholder="Email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 placeholder:text-zinc-500 focus:outline-none focus:border-amber-500"
            />
            <input
              type="password"
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-sm text-zinc-200 placeholder:text-zinc-500 focus:outline-none focus:border-amber-500"
            />
            <button
              type="submit"
              disabled={loading}
              className="w-full py-2.5 bg-amber-500 hover:bg-amber-600 text-zinc-900 font-medium rounded-lg text-sm transition-colors disabled:opacity-50"
            >
              {loading ? '...' : isRegister ? 'Create Account' : 'Sign In'}
            </button>
          </form>

          <div className="mt-4 pt-4 border-t border-zinc-800">
            <button
              onClick={handleDemo}
              disabled={loading}
              className="w-full py-2.5 bg-zinc-800 hover:bg-zinc-700 text-amber-400 font-medium rounded-lg text-sm transition-colors border border-zinc-700 disabled:opacity-50"
            >
              Try Demo
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
