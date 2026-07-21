import React, { useCallback, useState, useEffect } from 'react';
import { request, HttpError, setTokenGetter } from '../services/httpClient';
import type { AuthSession, AuthLoginResponse } from '../types/auth';
import { AuthContext } from './authContextDefinition';
import type { AuthContextType } from './authContextDefinition';

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [session, setSession] = useState<AuthSession | null>(null);
  const [isAuthenticating, setIsAuthenticating] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const tokenRef = React.useRef<string | null>(null);

  // Bind the token getter to httpClient using a stable reference
  useEffect(() => {
    setTokenGetter(() => tokenRef.current);
    return () => {
      setTokenGetter(() => null);
    };
  }, []);

  // Handle auto-expiration timer using UTC time comparison
  useEffect(() => {
    if (!session) return;

    const expiresTime = new Date(session.expiresAtUtc).getTime();

    // Check if the parsed date is valid
    if (isNaN(expiresTime)) {
      logout();
      return;
    }

    const currentTime = Date.now();
    const timeUntilExpiry = expiresTime - currentTime;

    if (timeUntilExpiry <= 0) {
      logout();
      return;
    }

    const timer = setTimeout(() => {
      logout();
    }, timeUntilExpiry);

    return () => clearTimeout(timer);
  }, [session]);

  const login = async (correo: string, clave: string) => {
    setIsAuthenticating(true);
    setError(null);
    try {
      const response = await request<AuthLoginResponse>('api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ correo, clave })
      });

      tokenRef.current = response.accessToken;

      setSession({
        user: response.user,
        accessToken: response.accessToken,
        expiresAtUtc: response.expiresAtUtc
      });
    } catch (err) {
      if (err instanceof HttpError) {
        if (err.status === 400) {
          setError('Revisa los datos ingresados.');
        } else if (err.status === 401) {
          setError('Correo o contraseña incorrectos.');
        } else if (err.status === 403) {
          setError('No tienes acceso habilitado para esta aplicación.');
        } else {
          setError('Ocurrió un error inesperado. Intenta nuevamente.');
        }
      } else {
        setError('No fue posible conectar con el servidor.');
      }
      throw err;
    } finally {
      setIsAuthenticating(false);
    }
  };

  const logout = () => {
    tokenRef.current = null;
    setSession(null);
    setError(null);
  };

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const value: AuthContextType = {
    user: session?.user ?? null,
    accessToken: session?.accessToken ?? null,
    expiresAtUtc: session?.expiresAtUtc ?? null,
    isAuthenticated: !!session,
    isAuthenticating,
    error,
    login,
    logout,
    clearError
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
