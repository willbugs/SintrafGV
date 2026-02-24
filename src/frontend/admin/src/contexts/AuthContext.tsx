import React, { createContext, useContext, useState, useEffect } from 'react';
import type { ReactNode } from 'react';
import type { User, AuthContextType, LoginCredentials } from '../types';
import { authAPI } from '../services/api';

const STORAGE_TOKEN = 'sintrafgv_token';
const STORAGE_USER = 'sintrafgv_user';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [loginLoading, setLoginLoading] = useState(false);

  const logout = () => {
    localStorage.removeItem(STORAGE_TOKEN);
    localStorage.removeItem(STORAGE_USER);
    setUser(null);
    setLoginLoading(false);
  };

  useEffect(() => {
    const token = localStorage.getItem(STORAGE_TOKEN);
    const userData = localStorage.getItem(STORAGE_USER);
    if (token && userData) {
      try {
        setUser(JSON.parse(userData));
      } catch {
        logout();
      }
    }
    setLoading(false);
  }, []);

  const login = async (credentials: LoginCredentials): Promise<void> => {
    setLoginLoading(true);
    try {
      const response = await authAPI.login(credentials) as { success?: boolean; data?: { token: string; user: User } };
      if (response?.success && response.data?.token && response.data?.user) {
        const { token, user: userData } = response.data;
        localStorage.setItem(STORAGE_TOKEN, token);
        localStorage.setItem(STORAGE_USER, JSON.stringify(userData));
        setUser(userData);
      } else {
        throw new Error('Credenciais inválidas.');
      }
    } catch (err: unknown) {
      const message = err && typeof err === 'object' && 'response' in err
        ? (err as { response?: { data?: { message?: string } } }).response?.data?.message
        : null;
      throw new Error(message || 'Erro ao fazer login. Verifique a API.');
    } finally {
      setLoginLoading(false);
    }
  };

  return (
    <AuthContext.Provider value={{ user, login, logout, loading, loginLoading }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth deve ser usado dentro de AuthProvider');
  }
  return context;
};
