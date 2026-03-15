import { create } from 'zustand';

interface UserInfo {
  id: string;
  email: string;
  displayName: string | null;
}

interface AuthState {
  user: UserInfo | null;
  isAuthenticated: boolean;
  login: (accessToken: string, refreshToken: string, user: UserInfo) => void;
  logout: () => void;
  hydrate: () => void;
}

export const useAuth = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,

  login: (accessToken, refreshToken, user) => {
    localStorage.setItem('access_token', accessToken);
    localStorage.setItem('refresh_token', refreshToken);
    localStorage.setItem('user', JSON.stringify(user));
    set({ user, isAuthenticated: true });
  },

  logout: () => {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user');
    set({ user: null, isAuthenticated: false });
  },

  hydrate: () => {
    const token = localStorage.getItem('access_token');
    const userJson = localStorage.getItem('user');
    if (token && userJson) {
      try {
        set({ user: JSON.parse(userJson), isAuthenticated: true });
      } catch {
        localStorage.clear();
      }
    }
  },
}));
