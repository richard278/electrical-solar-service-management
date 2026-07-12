import React, { useState, useRef, useEffect } from 'react';
import { useAuth } from '../auth/useAuth';

export default function LoginPage() {
  const { login, isAuthenticating, error, clearError } = useAuth();

  const [correo, setCorreo] = useState('');
  const [clave, setClave] = useState('');
  const [validationError, setValidationError] = useState<string | null>(null);

  const correoRef = useRef<HTMLInputElement>(null);
  const claveRef = useRef<HTMLInputElement>(null);

  // Clear errors when user types
  useEffect(() => {
    if (correo || clave) {
      setValidationError(null);
      clearError();
    }
  }, [correo, clave]);

  // Focus the email field on mount
  useEffect(() => {
    correoRef.current?.focus();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedCorreo = correo.trim();
    if (!trimmedCorreo) {
      setValidationError('El correo electrónico es obligatorio.');
      correoRef.current?.focus();
      return;
    }

    if (!clave) {
      setValidationError('La contraseña es obligatoria.');
      claveRef.current?.focus();
      return;
    }

    // Simple email regex validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(trimmedCorreo)) {
      setValidationError('Ingrese un formato de correo electrónico válido.');
      correoRef.current?.focus();
      return;
    }

    try {
      await login(trimmedCorreo, clave);
    } catch (err) {
      // Error is stored in AuthContext, but we clean password and focus password field
      setClave('');
      setTimeout(() => {
        claveRef.current?.focus();
      }, 50);
    }
  };

  const displayError = validationError || error;

  return (
    <div className="login-page-container">
      <style>{`
        .login-page-container {
          min-height: 100vh;
          display: flex;
          align-items: center;
          justify-content: center;
          background: linear-gradient(135deg, #0b2545 0%, #134074 50%, #0f172a 100%);
          padding: 20px;
          box-sizing: border-box;
          color: #f8fafc;
        }

        .login-card {
          width: 100%;
          max-width: 440px;
          background: rgba(15, 23, 42, 0.85);
          backdrop-filter: blur(16px);
          -webkit-backdrop-filter: blur(16px);
          border: 1px solid rgba(255, 255, 255, 0.1);
          border-radius: 16px;
          box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
          padding: 40px;
          box-sizing: border-box;
          transition: transform 0.3s ease, box-shadow 0.3s ease;
        }

        .login-card:hover {
          transform: translateY(-2px);
          box-shadow: 0 15px 35px rgba(15, 164, 122, 0.2);
          border-color: rgba(15, 164, 122, 0.3);
        }

        .login-brand {
          display: flex;
          flex-direction: column;
          align-items: center;
          margin-bottom: 30px;
          text-align: center;
        }

        .login-logo {
          font-size: 48px;
          margin-bottom: 12px;
          filter: drop-shadow(0 0 12px #0fa47a);
          animation: pulse 2s infinite ease-in-out;
        }

        @keyframes pulse {
          0% { transform: scale(1); }
          50% { transform: scale(1.05); }
          100% { transform: scale(1); }
        }

        .login-title {
          font-size: 24px;
          font-weight: 700;
          margin: 0 0 4px 0;
          color: #f8fafc;
          letter-spacing: -0.5px;
        }

        .login-subtitle {
          font-size: 13px;
          color: #94a3b8;
          margin: 0;
        }

        .login-form {
          display: flex;
          flex-direction: column;
          gap: 20px;
        }

        .form-group {
          display: flex;
          flex-direction: column;
          gap: 8px;
        }

        .form-label {
          font-size: 13px;
          font-weight: 600;
          color: #cbd5e1;
        }

        .form-input {
          background: rgba(15, 23, 42, 0.6);
          border: 1px solid #334155;
          border-radius: 8px;
          color: #f8fafc;
          padding: 12px 16px;
          font-size: 15px;
          font-family: inherit;
          transition: border-color 0.2s, box-shadow 0.2s;
          outline: none;
        }

        .form-input:focus {
          border-color: #0fa47a;
          box-shadow: 0 0 0 3px rgba(15, 164, 122, 0.2);
        }

        .form-input:disabled {
          opacity: 0.5;
          cursor: not-allowed;
        }

        .error-banner {
          background: rgba(239, 68, 68, 0.15);
          border: 1px solid rgba(239, 68, 68, 0.3);
          border-radius: 8px;
          color: #fca5a5;
          padding: 12px;
          font-size: 14px;
          line-height: 1.4;
        }

        .submit-button {
          background: #0fa47a;
          color: #ffffff;
          border: none;
          border-radius: 8px;
          padding: 14px;
          font-size: 15px;
          font-weight: 600;
          cursor: pointer;
          transition: background-color 0.2s, transform 0.1s;
          display: flex;
          align-items: center;
          justify-content: center;
          gap: 10px;
          outline: none;
        }

        .submit-button:hover:not(:disabled) {
          background: #0d8c68;
        }

        .submit-button:active:not(:disabled) {
          transform: scale(0.98);
        }

        .submit-button:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }

        .spinner-mini {
          width: 18px;
          height: 18px;
          border: 2px solid rgba(255, 255, 255, 0.3);
          border-top-color: #ffffff;
          border-radius: 50%;
          animation: spin 0.8s linear infinite;
        }

        @keyframes spin {
          to { transform: rotate(360deg); }
        }
      `}</style>

      <div className="login-card">
        <div className="login-brand">
          <div className="login-logo" aria-hidden="true">☀️</div>
          <h2 className="login-title">EcoInstalacion</h2>
          <p className="login-subtitle">Electrical Solar Service Management</p>
        </div>

        <form className="login-form" onSubmit={handleSubmit} noValidate>
          <div className="form-group">
            <label className="form-label" htmlFor="correo">
              Correo Electrónico
            </label>
            <input
              ref={correoRef}
              className="form-input"
              type="email"
              id="correo"
              name="correo"
              value={correo}
              onChange={(e) => setCorreo(e.target.value)}
              placeholder="nombre@ecoinstalador.com"
              disabled={isAuthenticating}
              autoComplete="username"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label" htmlFor="clave">
              Contraseña
            </label>
            <input
              ref={claveRef}
              className="form-input"
              type="password"
              id="clave"
              name="clave"
              value={clave}
              onChange={(e) => setClave(e.target.value)}
              placeholder="••••••••"
              disabled={isAuthenticating}
              autoComplete="current-password"
              required
            />
          </div>

          {displayError && (
            <div className="error-banner" role="alert">
              ⚠️ {displayError}
            </div>
          )}

          <button
            className="submit-button"
            type="submit"
            disabled={isAuthenticating}
          >
            {isAuthenticating ? (
              <>
                <div className="spinner-mini" />
                <span>Iniciando sesión...</span>
              </>
            ) : (
              <span>Iniciar Sesión</span>
            )}
          </button>
        </form>
      </div>
    </div>
  );
}
