import React, { useState, useEffect, useRef, useId } from 'react';
import {
  Save,
  X,
  AlertTriangle
} from 'lucide-react';
import type {
  ClienteDetail,
  ClienteCreatePayload,
  ClienteUpdatePayload
} from '../types';
import { HttpError } from '../../../services/httpClient';
import '../../../components/common/MutationUi.css';

interface ClienteFormModalProps {
  isOpen: boolean;
  mode: 'create' | 'edit';
  initialData?: ClienteDetail | null;
  isLoadingDetail?: boolean;
  /** true cuando el usuario logueado es Administrador */
  isAdmin: boolean;
  onSave: (payload: ClienteCreatePayload | ClienteUpdatePayload) => Promise<void>;
  onClose: () => void;
}

interface FormState {
  nombreCompleto: string;
  dui: string;
  telefono: string;
  direccion: string;
  correo: string;
  esActivo: boolean;
}

interface ValidationErrors {
  nombreCompleto?: string;
  dui?: string;
  telefono?: string;
  direccion?: string;
  correo?: string;
}

export const ClienteFormModal: React.FC<ClienteFormModalProps> = ({
  isOpen,
  mode,
  initialData,
  isLoadingDetail = false,
  isAdmin,
  onSave,
  onClose
}) => {
  const uid = useId();
  const titleId = `${uid}-title`;
  const serverErrorId = `${uid}-server-error`;
  const fieldErrorId = (field: string) => `${uid}-err-${field}`;
  const estadoHelpId = `${uid}-esActivo-help`;

  const dialogRef = useRef<HTMLDivElement>(null);
  const firstFieldRef = useRef<HTMLInputElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);

  const [form, setForm] = useState<FormState>({
    nombreCompleto: '',
    dui: '',
    telefono: '',
    direccion: '',
    correo: '',
    esActivo: true
  });

  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [serverFieldErrors, setServerFieldErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  // Efecto A: Restaurar foco (depende solo de isOpen)
  useEffect(() => {
    if (isOpen) {
      previousFocusRef.current = document.activeElement as HTMLElement;
    } else {
      previousFocusRef.current?.focus();
      previousFocusRef.current = null;
    }
  }, [isOpen]);

  // Efecto B: Inicializar formulario (depende de isOpen, mode, initialData)
  useEffect(() => {
    if (isOpen) {
      if (mode === 'edit' && initialData) {
        setForm({
          nombreCompleto: initialData.nombreCompleto || '',
          dui: initialData.dui || '',
          telefono: initialData.telefono || '',
          direccion: initialData.direccion || '',
          correo: initialData.correo || '',
          esActivo: initialData.esActivo
        });
      } else {
        setForm({
          nombreCompleto: '',
          dui: '',
          telefono: '',
          direccion: '',
          correo: '',
          esActivo: true
        });
      }
      setValidationErrors({});
      setServerError(null);
      setServerFieldErrors([]);
      setIsSubmitting(false);
    }
  }, [isOpen, mode, initialData]);

  useEffect(() => {
    if (isOpen) {
      if (isLoadingDetail) {
        requestAnimationFrame(() => {
          dialogRef.current?.focus();
        });
      } else {
        requestAnimationFrame(() => {
          firstFieldRef.current?.focus();
        });
      }
    }
  }, [isOpen, isLoadingDetail]);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (!isSubmitting) {
          onClose();
        }
        return;
      }

      if (e.key === 'Tab') {
        const focusableNodes = dialogRef.current?.querySelectorAll(
          'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
        );

        let focusableElements: HTMLElement[] = [];
        if (focusableNodes) {
          focusableElements = Array.from(focusableNodes) as HTMLElement[];
        }

        if (focusableElements.length === 0) {
          e.preventDefault();
          dialogRef.current?.focus();
          return;
        }

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        const activeElement = document.activeElement as HTMLElement;
        const isOutside = !dialogRef.current?.contains(activeElement);

        if (isOutside) {
          e.preventDefault();
          if (e.shiftKey) {
            lastElement.focus();
          } else {
            firstElement.focus();
          }
          return;
        }

        if (e.shiftKey) {
          if (activeElement === firstElement) {
            lastElement.focus();
            e.preventDefault();
          }
        } else {
          if (activeElement === lastElement) {
            firstElement.focus();
            e.preventDefault();
          }
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isSubmitting, onClose]);

  if (!isOpen) return null;

  const validate = (): boolean => {
    const errors: ValidationErrors = {};

    const nombreTrim = form.nombreCompleto.trim();
    if (!nombreTrim) {
      errors.nombreCompleto = 'El nombre completo es obligatorio.';
    } else if (nombreTrim.length > 100) {
      errors.nombreCompleto = 'El nombre completo no debe exceder 100 caracteres.';
    }

    const duiTrim = form.dui.trim();
    if (!duiTrim) {
      errors.dui = 'El DUI es obligatorio.';
    } else if (duiTrim.length > 10) {
      errors.dui = 'El DUI no debe exceder 10 caracteres.';
    }

    const telTrim = form.telefono.trim();
    if (telTrim && telTrim.length > 15) {
      errors.telefono = 'El teléfono no debe exceder 15 caracteres.';
    }

    const dirTrim = form.direccion.trim();
    if (dirTrim && dirTrim.length > 150) {
      errors.direccion = 'La dirección no debe exceder 150 caracteres.';
    }

    const mailTrim = form.correo.trim();
    if (mailTrim) {
      if (mailTrim.length > 100) {
        errors.correo = 'El correo no debe exceder 100 caracteres.';
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(mailTrim)) {
        errors.correo = 'El correo no tiene un formato válido.';
      }
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (validationErrors[name as keyof ValidationErrors]) {
      setValidationErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const handleEsActivoChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setForm((prev) => ({ ...prev, esActivo: e.target.value === 'true' }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setServerError(null);
    setServerFieldErrors([]);

    if (!validate()) return;

    setIsSubmitting(true);
    try {
      if (mode === 'create') {
        const payload: ClienteCreatePayload = {
          nombreCompleto: form.nombreCompleto.trim(),
          dui: form.dui.trim(),
          telefono: form.telefono.trim() || undefined,
          direccion: form.direccion.trim() || undefined,
          correo: form.correo.trim() || undefined
          // esActivo no incluido: backend fuerza true
        };
        await onSave(payload);
      } else {
        const payload: ClienteUpdatePayload = {
          nombreCompleto: form.nombreCompleto.trim(),
          dui: form.dui.trim(),
          telefono: form.telefono.trim() || undefined,
          direccion: form.direccion.trim() || undefined,
          correo: form.correo.trim() || undefined,
          esActivo: form.esActivo
        };
        await onSave(payload);
      }
    } catch (err) {
      if (err instanceof HttpError) {
        if (err.status === 400) {
          if (err.validationErrors && Object.keys(err.validationErrors).length > 0) {
            const flatMessages: string[] = [];
            for (const messages of Object.values(err.validationErrors)) {
              for (const msg of messages) {
                if (!flatMessages.includes(msg)) flatMessages.push(msg);
              }
            }
            setServerError('Se encontraron errores de validación:');
            setServerFieldErrors(flatMessages);
          } else {
            setServerError(err.message || 'Error de validación en la solicitud.');
          }
        } else if (err.status === 403) {
          setServerError('Acceso Denegado: No posees permisos suficientes para realizar esta acción.');
        } else if (err.status === 404) {
          setServerError('El cliente solicitado no existe o fue eliminado. El listado se actualizará al cerrar.');
        } else if (err.status === 401) {
          setServerError(err.message || 'Sesión no autorizada o expirada.');
        } else {
          setServerError(err.message || 'Ocurrió un error inesperado al procesar el cliente.');
        }
      } else if (err instanceof Error) {
        setServerError(err.message);
      } else {
        setServerError('No fue posible conectar con el servidor.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const hasServerError = serverError !== null;
  // Mostrar control esActivo solo al editar como Administrador
  const showEstadoControl = mode === 'edit' && isAdmin;

  return (
    <div
      className="mutation-modal-overlay"
      onClick={() => {
        if (!isSubmitting) onClose();
      }}
    >
      <div
        ref={dialogRef}
        className="mutation-modal-dialog"
        role="dialog"
        aria-modal="true"
        tabIndex={-1}
        aria-labelledby={titleId}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mutation-modal-header">
          <h3 id={titleId} className="mutation-modal-title">
            {mode === 'create' ? 'Agregar Nuevo Cliente' : 'Editar Cliente'}
          </h3>
          <button
            type="button"
            className="mutation-modal-close-btn"
            aria-label="Cerrar modal"
            onClick={onClose}
            disabled={isSubmitting}
          >
            <X size={18} aria-hidden="true" />
          </button>
        </div>

        {isLoadingDetail ? (
          <div className="mutation-modal-body mutation-modal-loading">
            <div className="spinner"></div>
            <p className="mutation-modal-loading-text">
              Cargando detalle del cliente...
            </p>
          </div>
        ) : (
          <form onSubmit={handleSubmit} noValidate>
            <div className="mutation-modal-body">
              {hasServerError && (
                <div
                  id={serverErrorId}
                  className="mutation-error-banner"
                  role="alert"
                  aria-live="assertive"
                >
                  <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
                  <div>
                    <strong>Error:</strong> {serverError}
                    {serverFieldErrors.length > 0 && (
                      <ul className="mutation-error-list">
                        {serverFieldErrors.map((msg, idx) => (
                          <li key={idx}>{msg}</li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>
              )}

              <div className="mutation-form-grid">
                {/* Nombre Completo */}
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-nombreCompleto`} className="mutation-form-label">
                    Nombre Completo <span className="required-star" aria-hidden="true">*</span>
                  </label>
                  <input
                    ref={firstFieldRef}
                    type="text"
                    id={`${uid}-nombreCompleto`}
                    name="nombreCompleto"
                    className={`mutation-form-control ${validationErrors.nombreCompleto ? 'is-invalid' : ''}`}
                    maxLength={100}
                    value={form.nombreCompleto}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. Juan Carlos Pérez"
                    required
                    aria-required="true"
                    aria-invalid={!!validationErrors.nombreCompleto}
                    aria-describedby={
                      validationErrors.nombreCompleto
                        ? fieldErrorId('nombreCompleto')
                        : hasServerError
                        ? serverErrorId
                        : undefined
                    }
                  />
                  {validationErrors.nombreCompleto && (
                    <p id={fieldErrorId('nombreCompleto')} className="mutation-field-error" role="alert">
                      {validationErrors.nombreCompleto}
                    </p>
                  )}
                </div>

                {/* DUI */}
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-dui`} className="mutation-form-label">
                    DUI <span className="required-star" aria-hidden="true">*</span>
                  </label>
                  <input
                    type="text"
                    id={`${uid}-dui`}
                    name="dui"
                    className={`mutation-form-control ${validationErrors.dui ? 'is-invalid' : ''}`}
                    maxLength={10}
                    value={form.dui}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. 01234567-8"
                    required
                    aria-required="true"
                    aria-invalid={!!validationErrors.dui}
                    aria-describedby={validationErrors.dui ? fieldErrorId('dui') : undefined}
                  />
                  {validationErrors.dui && (
                    <p id={fieldErrorId('dui')} className="mutation-field-error" role="alert">
                      {validationErrors.dui}
                    </p>
                  )}
                </div>

                {/* Teléfono */}
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-telefono`} className="mutation-form-label">Teléfono</label>
                  <input
                    type="text"
                    id={`${uid}-telefono`}
                    name="telefono"
                    className={`mutation-form-control ${validationErrors.telefono ? 'is-invalid' : ''}`}
                    maxLength={15}
                    value={form.telefono}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. 7890-1234"
                    aria-invalid={!!validationErrors.telefono}
                    aria-describedby={validationErrors.telefono ? fieldErrorId('telefono') : undefined}
                  />
                  {validationErrors.telefono && (
                    <p id={fieldErrorId('telefono')} className="mutation-field-error" role="alert">
                      {validationErrors.telefono}
                    </p>
                  )}
                </div>

                {/* Correo */}
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-correo`} className="mutation-form-label">Correo Electrónico</label>
                  <input
                    type="email"
                    id={`${uid}-correo`}
                    name="correo"
                    className={`mutation-form-control ${validationErrors.correo ? 'is-invalid' : ''}`}
                    maxLength={100}
                    value={form.correo}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. cliente@correo.com"
                    aria-invalid={!!validationErrors.correo}
                    aria-describedby={validationErrors.correo ? fieldErrorId('correo') : undefined}
                  />
                  {validationErrors.correo && (
                    <p id={fieldErrorId('correo')} className="mutation-field-error" role="alert">
                      {validationErrors.correo}
                    </p>
                  )}
                </div>

                {/* Dirección */}
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-direccion`} className="mutation-form-label">Dirección Residencial</label>
                  <textarea
                    id={`${uid}-direccion`}
                    name="direccion"
                    rows={3}
                    className={`mutation-form-control ${validationErrors.direccion ? 'is-invalid' : ''}`}
                    maxLength={150}
                    value={form.direccion}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. Col. Escalón, Calle Principal #12"
                    aria-invalid={!!validationErrors.direccion}
                    aria-describedby={validationErrors.direccion ? fieldErrorId('direccion') : undefined}
                  />
                  {validationErrors.direccion && (
                    <p id={fieldErrorId('direccion')} className="mutation-field-error" role="alert">
                      {validationErrors.direccion}
                    </p>
                  )}
                </div>

                {/* Estado activo/inactivo — solo Admin en modo edición */}
                {showEstadoControl && (
                  <div className="mutation-form-group">
                    <label htmlFor={`${uid}-esActivo`} className="mutation-form-label">
                      Estado del Cliente
                    </label>
                    <select
                      id={`${uid}-esActivo`}
                      name="esActivo"
                      className="mutation-form-control"
                      value={form.esActivo ? 'true' : 'false'}
                      onChange={handleEsActivoChange}
                      disabled={isSubmitting}
                      aria-describedby={estadoHelpId}
                    >
                      <option value="true">Activo</option>
                      <option value="false">Inactivo</option>
                    </select>
                    <p id={estadoHelpId} className="mutation-field-help">
                      Los clientes inactivos no estarán disponibles para la operación técnica.
                    </p>
                  </div>
                )}
              </div>
            </div>

            <div className="mutation-modal-footer">
              <button
                type="button"
                className="mutation-btn mutation-btn-secondary"
                onClick={onClose}
                disabled={isSubmitting}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="mutation-btn mutation-btn-primary"
                disabled={isSubmitting}
              >
                {isSubmitting ? (
                  <>
                    <span className="mutation-spinner" aria-hidden="true"></span>
                    <span>Guardando...</span>
                  </>
                ) : (
                  <>
                    <Save size={16} aria-hidden="true" />
                    <span>{mode === 'create' ? 'Guardar Cliente' : 'Actualizar Cliente'}</span>
                  </>
                )}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
