import { createContext, useState, useEffect, type ReactNode } from 'react';
import { getAccessToken, setAccessToken } from '../api/client';
import * as authApi from '../api/auth';
import type { User } from '../types';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<string | null>;
  getCurrentUser: () => Promise<User | null>;
  devLogin: (role: 'Admin' | 'Parent') => void;
}

export const AuthContext = createContext<AuthState>({
  user: null, isAuthenticated: false, isLoading: true,
  login: async () => {}, logout: async () => {},
  refreshToken: async () => null, getCurrentUser: async () => null,
  devLogin: () => {},
});

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const init = async () => {
      const token = getAccessToken();
      if (token) {
        try {
          const me = await authApi.getMe();
          setUser(me);
        } catch { setAccessToken(null); }
      }
      setIsLoading(false);
    };
    init();
  }, []);

  const login = async (email: string, password: string) => {
    const res = await authApi.login(email, password);
    setAccessToken(res.accessToken);
    setUser(res.user);
  };

  const logout = async () => {
    try { await authApi.logout(); } catch { /* ignore */ }
    setAccessToken(null);
    setUser(null);
  };

  const refreshToken = async (): Promise<string | null> => {
    try {
      const res = await authApi.refreshToken();
      setAccessToken(res.accessToken);
      return res.accessToken;
    } catch { return null; }
  };

  const getCurrentUser = async (): Promise<User | null> => {
    try {
      const me = await authApi.getMe();
      setUser(me);
      return me;
    } catch { return null; }
  };

  const devLogin = (role: 'Admin' | 'Parent') => {
    setAccessToken('dev-token');
    setUser({ id: 'dev-1', name: role === 'Admin' ? 'Admin Dev' : 'Pai Dev', email: 'dev@edugestor.com', role, tenantId: 'dev-tenant' });
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, logout, refreshToken, getCurrentUser, devLogin }}>
      {children}
    </AuthContext.Provider>
  );
}
