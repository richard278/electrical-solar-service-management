import { createContext } from 'react';
import type { AuthUser } from '../types/auth';

export interface AuthContextType {
  user: AuthUser | null;
  accessToken: string | null;
  expiresAtUtc: string | null;
  isAuthenticated: boolean;
  isAuthenticating: boolean;
  error: string | null;
  login: (correo: string, clave: string) => Promise<void>;
  logout: () => void;
  clearError: () => void;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
