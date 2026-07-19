import React, { useEffect, useId, useRef, useState } from 'react';
import { AlertTriangle, Save, X } from 'lucide-react';
import type {
  TecnicoCreatePayload,
  TecnicoDetail,
  TecnicoUpdatePayload
} from '../types';
import { HttpError } from '../../../services/httpClient';
import '../../../components/common/MutationUi.css';

interface TecnicoFormModalProps {
  isOpen: boolean;
  mode: 'create' | 'edit';
  initialData?: TecnicoDetail | null;
  isLoadingDetail?: boolean;
  onSave: (payload: TecnicoCreatePayload | TecnicoUpdatePayload) => Promise<void>;
  onClose: () => void;
}

interface FormState {
  nombre: string;
  especialidad: string;
  telefono: string;
  correo: string;
}

interface ValidationErrors {
  nombre?: string;
  especialidad?: string;
  telefono?: string;
  correo?: string;
}

const EMPTY_FORM: FormState = {
  nombre: '',
  especialidad: '',
  telefono: '',
  correo: ''
};

export const TecnicoFormModal: React.FC<TecnicoFormModalProps> = ({
  isOpen,
  mode,
  initialData,
  isLoadingDetail = false,
  onSave,
  onClose
}) => {
  const uid = useId();
  const titleId = `${uid}-title`;
  const serverErrorId = `${uid}-server-error`;
  const fieldErrorId = (field: string) => `${uid}-err-${field}`;

  const dialogRef = useRef<HTMLDivElement>(null);
  const firstFieldRef = useRef<HTMLInputElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);

  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [serverFieldErrors, setServerFieldErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  useEffect(() => {
    if (isOpen) {
      previousFocusRef.current = document.activeElement as HTMLElement;
    } else {
      previousFocusRef.current?.focus();
      previousFocusRef.current = null;
    }
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) return;

    if (mode === 'edit' && initialData) {
      setForm({
        nombre: initialData.nombre || '',
        especialidad: initialData.especialidad || '',
        telefono: initialData.telefono || '',
        correo: initialData.correo || ''
      });
    } else {
      setForm(EMPTY_FORM);
    }

    setValidationErrors({});
    setServerError(null);
    setServerFieldErrors([]);
    setIsSubmitting(false);
  }, [isOpen, mode, initialData]);

  useEffect(() => {
    if (!isOpen) return;

    requestAnimationFrame(() => {
      if (isLoadingDetail) {
        dialogRef.current?.focus();
      } else {
        firstFieldRef.current?.focus();
      }
    });
  }, [isOpen, isLoadingDetail]);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        if (!isSubmitting) onClose();
        return;
      }

      if (event.key !== 'Tab') return;

      const nodes = dialogRef.current?.querySelectorAll(
        'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
      );
      const elements = nodes ? (Array.from(nodes) as HTMLElement[]) : [];

      if (elements.length === 0) {
        event.preventDefault();
        dialogRef.current?.focus();
        return;
      }

      const first = elements[0];
      const last = elements[elements.length - 1];
      const active = document.activeElement as HTMLElement;

      if (!dialogRef.current?.contains(active)) {
        event.preventDefault();
        (event.shiftKey ? last : first).focus();
      } else if (event.shiftKey && active === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first.focus();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isSubmitting, onClose]);

  if (!isOpen) return null;

  const validate = (): boolean => {
    const errors: ValidationErrors = {};
    const nombre = form.nombre.trim();
    const especialidad = form.especialidad.trim();
    const telefono = form.telefono.trim();
    const correo = form.correo.trim();

    if (!nombre) {
      errors.nombre = 'El nombre del técnico es obligatorio.';
    } else if (nombre.length > 100) {
      errors.nombre = 'El nombre no debe exceder 100 caracteres.';
    }

    if (especialidad.length > 50) {
      errors.especialidad = 'La especialidad no debe exceder 50 caracteres.';
    }

    if (telefono.length > 15) {
      errors.telefono = 'El teléfono no debe exceder 15 caracteres.';
    }

    if (correo) {
      if (correo.length > 100) {
        errors.correo = 'El correo no debe exceder 100 caracteres.';
      } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo)) {
        errors.correo = 'El correo no tiene un formato válido.';
      }
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setForm((previous) => ({ ...previous, [name]: value }));

    if (validationErrors[name as keyof ValidationErrors]) {
      setValidationErrors((previous) => ({ ...previous, [name]: undefined }));
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setServerError(null);
    setServerFieldErrors([]);

    if (!validate()) return;

    const payload: TecnicoCreatePayload | TecnicoUpdatePayload = {
      nombre: form.nombre.trim(),
      especialidad: form.especialidad.trim() || undefined,
      telefono: form.telefono.trim() || undefined,
      correo: form.correo.trim() || undefined
    };

    setIsSubmitting(true);
    try {
      await onSave(payload);
    } catch (error) {
      if (error instanceof HttpError) {
        if (error.status === 400 && error.validationErrors) {
          const messages = Array.from(new Set(Object.values(error.validationErrors).flat()));
          setServerError('Se encontraron errores de validación:');
          setServerFieldErrors(messages);
        } else if (error.status === 400) {
          setServerError(error.message || 'Error de validación en la solicitud.');
        } else if (error.status === 403) {
          setServerError('Acceso Denegado: No posees permisos para administrar técnicos.');
        } else if (error.status === 404) {
          setServerError('El técnico solicitado no existe o fue eliminado.');
        } else if (error.status === 401) {
          setServerError(error.message || 'Sesión no autorizada o expirada.');
        } else {
          setServerError(error.message || 'Ocurrió un error al procesar el técnico.');
        }
      } else if (error instanceof Error) {
        setServerError(error.message);
      } else {
        setServerError('No fue posible conectar con el servidor.');
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const hasServerError = serverError !== null;

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
        onClick={(event) => event.stopPropagation()}
      >
        <div className="mutation-modal-header">
          <h3 id={titleId} className="mutation-modal-title">
            {mode === 'create' ? 'Agregar Nuevo Técnico' : 'Editar Técnico'}
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
            <p className="mutation-modal-loading-text">Cargando detalle del técnico...</p>
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
                        {serverFieldErrors.map((message) => (
                          <li key={message}>{message}</li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>
              )}

              <div className="mutation-form-grid">
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-nombre`} className="mutation-form-label">
                    Nombre Completo <span className="required-star" aria-hidden="true">*</span>
                  </label>
                  <input
                    ref={firstFieldRef}
                    id={`${uid}-nombre`}
                    name="nombre"
                    type="text"
                    className={`mutation-form-control ${validationErrors.nombre ? 'is-invalid' : ''}`}
                    maxLength={100}
                    value={form.nombre}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. Fernando Palacios"
                    required
                    aria-required="true"
                    aria-invalid={!!validationErrors.nombre}
                    aria-describedby={
                      validationErrors.nombre
                        ? fieldErrorId('nombre')
                        : hasServerError
                        ? serverErrorId
                        : undefined
                    }
                  />
                  {validationErrors.nombre && (
                    <p id={fieldErrorId('nombre')} className="mutation-field-error" role="alert">
                      {validationErrors.nombre}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-especialidad`} className="mutation-form-label">
                    Especialidad
                  </label>
                  <input
                    id={`${uid}-especialidad`}
                    name="especialidad"
                    type="text"
                    className={`mutation-form-control ${validationErrors.especialidad ? 'is-invalid' : ''}`}
                    maxLength={50}
                    value={form.especialidad}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. Instalaciones solares"
                    aria-invalid={!!validationErrors.especialidad}
                    aria-describedby={validationErrors.especialidad ? fieldErrorId('especialidad') : undefined}
                  />
                  {validationErrors.especialidad && (
                    <p id={fieldErrorId('especialidad')} className="mutation-field-error" role="alert">
                      {validationErrors.especialidad}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-telefono`} className="mutation-form-label">
                    Teléfono
                  </label>
                  <input
                    id={`${uid}-telefono`}
                    name="telefono"
                    type="text"
                    className={`mutation-form-control ${validationErrors.telefono ? 'is-invalid' : ''}`}
                    maxLength={15}
                    value={form.telefono}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. 7012-1234"
                    aria-invalid={!!validationErrors.telefono}
                    aria-describedby={validationErrors.telefono ? fieldErrorId('telefono') : undefined}
                  />
                  {validationErrors.telefono && (
                    <p id={fieldErrorId('telefono')} className="mutation-field-error" role="alert">
                      {validationErrors.telefono}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-correo`} className="mutation-form-label">
                    Correo Electrónico
                  </label>
                  <input
                    id={`${uid}-correo`}
                    name="correo"
                    type="email"
                    className={`mutation-form-control ${validationErrors.correo ? 'is-invalid' : ''}`}
                    maxLength={100}
                    value={form.correo}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. tecnico@ecoinstalador.com"
                    aria-invalid={!!validationErrors.correo}
                    aria-describedby={validationErrors.correo ? fieldErrorId('correo') : undefined}
                  />
                  {validationErrors.correo && (
                    <p id={fieldErrorId('correo')} className="mutation-field-error" role="alert">
                      {validationErrors.correo}
                    </p>
                  )}
                </div>

                <p className="mutation-field-help">
                  Este registro pertenece al catálogo operativo. El acceso al sistema se administra por separado en Usuarios.
                </p>
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
                    <span>{mode === 'create' ? 'Guardar Técnico' : 'Actualizar Técnico'}</span>
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
