import React, { useEffect, useId, useRef, useState } from 'react';
import { AlertTriangle, Save, X } from 'lucide-react';
import { getClientes } from '../../clientes/clientesApi';
import type { ClienteListItem } from '../../clientes/types';
import { getTecnicos } from '../../tecnicos/tecnicosApi';
import type { TecnicoListItem } from '../../tecnicos/types';
import { HttpError } from '../../../services/httpClient';
import type {
  SolicitudCreatePayload,
  SolicitudDetail,
  SolicitudEstado,
  SolicitudUpdatePayload
} from '../types';
import { SOLICITUD_ESTADOS } from '../types';
import '../../../components/common/MutationUi.css';

type SolicitudModalMode = 'create' | 'edit' | 'view';

interface SolicitudFormModalProps {
  isOpen: boolean;
  mode: SolicitudModalMode;
  initialData?: SolicitudDetail | null;
  isLoadingDetail?: boolean;
  onSave: (payload: SolicitudCreatePayload | SolicitudUpdatePayload) => Promise<void>;
  onClose: () => void;
}

interface FormState {
  clienteId: string;
  tipoInstalacion: string;
  descripcion: string;
  tecnicoAsignadoId: string;
  estado: SolicitudEstado;
}

interface ValidationErrors {
  clienteId?: string;
  tipoInstalacion?: string;
  descripcion?: string;
  tecnicoAsignadoId?: string;
  estado?: string;
}

const EMPTY_FORM: FormState = {
  clienteId: '',
  tipoInstalacion: '',
  descripcion: '',
  tecnicoAsignadoId: '',
  estado: 'Pendiente'
};

function normalizarEstado(estado?: string | null): SolicitudEstado {
  const estadoNormalizado = estado?.trim().toLocaleLowerCase('es');
  return (
    SOLICITUD_ESTADOS.find(
      (estadoCanonico) => estadoCanonico.toLocaleLowerCase('es') === estadoNormalizado
    ) ?? 'Pendiente'
  );
}

function formatearFecha(fecha?: string | null): string {
  if (!fecha) return 'No disponible';

  const valor = new Date(fecha);
  if (Number.isNaN(valor.getTime())) return 'No disponible';

  return valor.toLocaleString('es-SV', {
    dateStyle: 'medium',
    timeStyle: 'short'
  });
}

export const SolicitudFormModal: React.FC<SolicitudFormModalProps> = ({
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
  const firstFieldRef = useRef<HTMLSelectElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  const catalogRequestIdRef = useRef<number>(0);

  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [clientes, setClientes] = useState<ClienteListItem[]>([]);
  const [tecnicos, setTecnicos] = useState<TecnicoListItem[]>([]);
  const [loadingCatalogs, setLoadingCatalogs] = useState<boolean>(false);
  const [catalogError, setCatalogError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [serverFieldErrors, setServerFieldErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  const isViewMode = mode === 'view';

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

    if ((mode === 'edit' || mode === 'view') && initialData) {
      setForm({
        clienteId: String(initialData.clienteId),
        tipoInstalacion: initialData.tipoInstalacion || '',
        descripcion: initialData.descripcion || '',
        tecnicoAsignadoId: initialData.tecnicoAsignadoId
          ? String(initialData.tecnicoAsignadoId)
          : '',
        estado: normalizarEstado(initialData.estado)
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
    if (!isOpen || isViewMode) return;

    const requestId = ++catalogRequestIdRef.current;
    setLoadingCatalogs(true);
    setCatalogError(null);

    void Promise.all([getClientes(), getTecnicos()])
      .then(([clientesData, tecnicosData]) => {
        if (requestId !== catalogRequestIdRef.current) return;
        setClientes(clientesData);
        setTecnicos(tecnicosData);
      })
      .catch((error: unknown) => {
        if (requestId !== catalogRequestIdRef.current) return;

        if (error instanceof HttpError) {
          setCatalogError(error.message || 'No fue posible cargar los catálogos del formulario.');
        } else if (error instanceof Error) {
          setCatalogError(error.message);
        } else {
          setCatalogError('No fue posible cargar los catálogos del formulario.');
        }
      })
      .finally(() => {
        if (requestId === catalogRequestIdRef.current) setLoadingCatalogs(false);
      });

    return () => {
      catalogRequestIdRef.current += 1;
    };
  }, [isOpen, isViewMode]);

  useEffect(() => {
    if (!isOpen) return;

    requestAnimationFrame(() => {
      if (isLoadingDetail || loadingCatalogs || isViewMode) {
        dialogRef.current?.focus();
      } else {
        firstFieldRef.current?.focus();
      }
    });
  }, [isOpen, isLoadingDetail, loadingCatalogs, isViewMode]);

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
    const clienteId = Number(form.clienteId);
    const tecnicoId = form.tecnicoAsignadoId ? Number(form.tecnicoAsignadoId) : null;
    const tipoInstalacion = form.tipoInstalacion.trim();
    const descripcion = form.descripcion.trim();

    if (!Number.isInteger(clienteId) || clienteId <= 0) {
      errors.clienteId = 'Debe seleccionar un cliente válido.';
    }

    if (!tipoInstalacion) {
      errors.tipoInstalacion = 'El tipo de instalación es obligatorio.';
    } else if (tipoInstalacion.length < 3 || tipoInstalacion.length > 50) {
      errors.tipoInstalacion = 'El tipo de instalación debe tener entre 3 y 50 caracteres.';
    }

    if (descripcion.length > 500) {
      errors.descripcion = 'La descripción no debe exceder 500 caracteres.';
    }

    if (tecnicoId !== null && (!Number.isInteger(tecnicoId) || tecnicoId <= 0)) {
      errors.tecnicoAsignadoId = 'Debe seleccionar un técnico válido.';
    }

    if (mode === 'edit' && !SOLICITUD_ESTADOS.includes(form.estado)) {
      errors.estado = 'Debe seleccionar un estado válido.';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleChange = (
    event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = event.target;
    setForm((previous) => ({ ...previous, [name]: value }));

    if (validationErrors[name as keyof ValidationErrors]) {
      setValidationErrors((previous) => ({ ...previous, [name]: undefined }));
    }
  };

  const handleEstadoChange = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setForm((previous) => ({
      ...previous,
      estado: event.target.value as SolicitudEstado
    }));
    setValidationErrors((previous) => ({ ...previous, estado: undefined }));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setServerError(null);
    setServerFieldErrors([]);

    if (!validate()) return;

    const basePayload: SolicitudCreatePayload = {
      clienteId: Number(form.clienteId),
      tipoInstalacion: form.tipoInstalacion.trim(),
      descripcion: form.descripcion.trim() || null,
      tecnicoAsignadoId: form.tecnicoAsignadoId
        ? Number(form.tecnicoAsignadoId)
        : null
    };

    const payload: SolicitudCreatePayload | SolicitudUpdatePayload =
      mode === 'edit'
        ? { ...basePayload, estado: form.estado }
        : basePayload;

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
          setServerError('Acceso Denegado: No posees permisos para administrar solicitudes.');
        } else if (error.status === 404) {
          setServerError('La solicitud o uno de sus registros relacionados ya no existe.');
        } else if (error.status === 401) {
          setServerError(error.message || 'Sesión no autorizada o expirada.');
        } else {
          setServerError(error.message || 'Ocurrió un error al procesar la solicitud.');
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

  const clienteActualNoDisponible =
    mode === 'edit' &&
    initialData &&
    !clientes.some((cliente) => cliente.id === initialData.clienteId);
  const tecnicoActualNoDisponible =
    mode === 'edit' &&
    initialData?.tecnicoAsignadoId &&
    !tecnicos.some((tecnico) => tecnico.id === initialData.tecnicoAsignadoId);
  const isLoading = isLoadingDetail || (!isViewMode && loadingCatalogs);
  const hasServerError = serverError !== null;

  const modalTitle =
    mode === 'create'
      ? 'Nueva Solicitud de Instalación'
      : mode === 'edit'
        ? 'Editar Solicitud de Instalación'
        : 'Detalle de Solicitud';

  return (
    <div
      className="mutation-modal-overlay"
      onClick={() => {
        if (!isSubmitting) onClose();
      }}
    >
      <div
        ref={dialogRef}
        className="mutation-modal-dialog mutation-modal-dialog-wide"
        role="dialog"
        aria-modal="true"
        tabIndex={-1}
        aria-labelledby={titleId}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="mutation-modal-header">
          <h3 id={titleId} className="mutation-modal-title">
            {modalTitle}
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

        {isLoading ? (
          <div className="mutation-modal-body mutation-modal-loading">
            <div className="spinner"></div>
            <p className="mutation-modal-loading-text">
              {isLoadingDetail
                ? 'Cargando detalle de la solicitud...'
                : 'Cargando clientes y técnicos...'}
            </p>
          </div>
        ) : isViewMode ? (
          <>
            <div className="mutation-modal-body">
              {initialData ? (
                <dl className="mutation-detail-grid">
                  <div className="mutation-detail-item">
                    <dt>Solicitud</dt>
                    <dd>#{initialData.id}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Fecha</dt>
                    <dd>{formatearFecha(initialData.fechaSolicitud)}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Cliente</dt>
                    <dd>{initialData.clienteNombre || `Cliente #${initialData.clienteId}`}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Tipo de instalación</dt>
                    <dd>{initialData.tipoInstalacion || 'No especificado'}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Técnico asignado</dt>
                    <dd>{initialData.tecnicoNombre || 'No asignado'}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Estado</dt>
                    <dd>{initialData.estado || 'No especificado'}</dd>
                  </div>
                  <div className="mutation-detail-item mutation-detail-item-wide">
                    <dt>Descripción</dt>
                    <dd>{initialData.descripcion || 'Sin descripción registrada.'}</dd>
                  </div>
                </dl>
              ) : (
                <div className="mutation-error-banner" role="alert">
                  <AlertTriangle size={15} aria-hidden="true" />
                  <span>No fue posible mostrar el detalle de la solicitud.</span>
                </div>
              )}
            </div>
            <div className="mutation-modal-footer">
              <button
                type="button"
                className="mutation-btn mutation-btn-secondary"
                onClick={onClose}
              >
                Cerrar
              </button>
            </div>
          </>
        ) : (
          <form onSubmit={handleSubmit} noValidate>
            <div className="mutation-modal-body">
              {(hasServerError || catalogError) && (
                <div
                  id={serverErrorId}
                  className="mutation-error-banner"
                  role="alert"
                  aria-live="assertive"
                >
                  <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
                  <div>
                    <strong>Error:</strong> {serverError || catalogError}
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

              <div className="mutation-form-grid mutation-form-grid-two">
                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-clienteId`} className="mutation-form-label">
                    Cliente <span className="required-star" aria-hidden="true">*</span>
                  </label>
                  <select
                    ref={firstFieldRef}
                    id={`${uid}-clienteId`}
                    name="clienteId"
                    className={`mutation-form-control ${validationErrors.clienteId ? 'is-invalid' : ''}`}
                    value={form.clienteId}
                    onChange={handleChange}
                    disabled={isSubmitting || !!catalogError}
                    required
                    aria-required="true"
                    aria-invalid={!!validationErrors.clienteId}
                    aria-describedby={
                      validationErrors.clienteId
                        ? fieldErrorId('clienteId')
                        : hasServerError || catalogError
                          ? serverErrorId
                          : undefined
                    }
                  >
                    <option value="">Seleccione un cliente</option>
                    {clienteActualNoDisponible && initialData && (
                      <option value={initialData.clienteId}>
                        {initialData.clienteNombre || `Cliente #${initialData.clienteId}`} (cliente actual)
                      </option>
                    )}
                    {clientes.map((cliente) => (
                      <option key={cliente.id} value={cliente.id}>
                        {cliente.nombreCompleto}
                      </option>
                    ))}
                  </select>
                  {validationErrors.clienteId && (
                    <p id={fieldErrorId('clienteId')} className="mutation-field-error" role="alert">
                      {validationErrors.clienteId}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-tecnicoAsignadoId`} className="mutation-form-label">
                    Técnico asignado
                  </label>
                  <select
                    id={`${uid}-tecnicoAsignadoId`}
                    name="tecnicoAsignadoId"
                    className={`mutation-form-control ${validationErrors.tecnicoAsignadoId ? 'is-invalid' : ''}`}
                    value={form.tecnicoAsignadoId}
                    onChange={handleChange}
                    disabled={isSubmitting || !!catalogError}
                    aria-invalid={!!validationErrors.tecnicoAsignadoId}
                    aria-describedby={
                      validationErrors.tecnicoAsignadoId
                        ? fieldErrorId('tecnicoAsignadoId')
                        : undefined
                    }
                  >
                    <option value="">Sin asignar</option>
                    {tecnicoActualNoDisponible && initialData?.tecnicoAsignadoId && (
                      <option value={initialData.tecnicoAsignadoId}>
                        {initialData.tecnicoNombre || `Técnico #${initialData.tecnicoAsignadoId}`} (técnico actual)
                      </option>
                    )}
                    {tecnicos.map((tecnico) => (
                      <option key={tecnico.id} value={tecnico.id}>
                        {tecnico.nombre}
                      </option>
                    ))}
                  </select>
                  {validationErrors.tecnicoAsignadoId && (
                    <p
                      id={fieldErrorId('tecnicoAsignadoId')}
                      className="mutation-field-error"
                      role="alert"
                    >
                      {validationErrors.tecnicoAsignadoId}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group mutation-form-group-wide">
                  <label htmlFor={`${uid}-tipoInstalacion`} className="mutation-form-label">
                    Tipo de instalación <span className="required-star" aria-hidden="true">*</span>
                  </label>
                  <input
                    id={`${uid}-tipoInstalacion`}
                    name="tipoInstalacion"
                    type="text"
                    className={`mutation-form-control ${validationErrors.tipoInstalacion ? 'is-invalid' : ''}`}
                    minLength={3}
                    maxLength={50}
                    value={form.tipoInstalacion}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Ej. Instalación solar residencial"
                    required
                    aria-required="true"
                    aria-invalid={!!validationErrors.tipoInstalacion}
                    aria-describedby={
                      validationErrors.tipoInstalacion
                        ? fieldErrorId('tipoInstalacion')
                        : undefined
                    }
                  />
                  {validationErrors.tipoInstalacion && (
                    <p
                      id={fieldErrorId('tipoInstalacion')}
                      className="mutation-field-error"
                      role="alert"
                    >
                      {validationErrors.tipoInstalacion}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group mutation-form-group-wide">
                  <label htmlFor={`${uid}-descripcion`} className="mutation-form-label">
                    Descripción
                  </label>
                  <textarea
                    id={`${uid}-descripcion`}
                    name="descripcion"
                    rows={4}
                    className={`mutation-form-control ${validationErrors.descripcion ? 'is-invalid' : ''}`}
                    maxLength={500}
                    value={form.descripcion}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Detalle del servicio solicitado y condiciones relevantes."
                    aria-invalid={!!validationErrors.descripcion}
                    aria-describedby={
                      validationErrors.descripcion
                        ? fieldErrorId('descripcion')
                        : `${uid}-descripcion-help`
                    }
                  />
                  <p id={`${uid}-descripcion-help`} className="mutation-field-help">
                    {form.descripcion.length}/500 caracteres
                  </p>
                  {validationErrors.descripcion && (
                    <p id={fieldErrorId('descripcion')} className="mutation-field-error" role="alert">
                      {validationErrors.descripcion}
                    </p>
                  )}
                </div>

                {mode === 'edit' ? (
                  <div className="mutation-form-group mutation-form-group-wide">
                    <label htmlFor={`${uid}-estado`} className="mutation-form-label">
                      Estado <span className="required-star" aria-hidden="true">*</span>
                    </label>
                    <select
                      id={`${uid}-estado`}
                      name="estado"
                      className={`mutation-form-control ${validationErrors.estado ? 'is-invalid' : ''}`}
                      value={form.estado}
                      onChange={handleEstadoChange}
                      disabled={isSubmitting}
                      required
                      aria-required="true"
                      aria-invalid={!!validationErrors.estado}
                      aria-describedby={`${uid}-estado-help`}
                    >
                      {SOLICITUD_ESTADOS.map((estado) => (
                        <option key={estado} value={estado}>
                          {estado}
                        </option>
                      ))}
                    </select>
                    <p id={`${uid}-estado-help`} className="mutation-field-help">
                      Una visita aprobada también cambia automáticamente el estado a “En proceso”.
                    </p>
                    {validationErrors.estado && (
                      <p id={fieldErrorId('estado')} className="mutation-field-error" role="alert">
                        {validationErrors.estado}
                      </p>
                    )}
                  </div>
                ) : (
                  <div className="mutation-form-group mutation-form-group-wide">
                    <span className="mutation-form-label">Estado inicial</span>
                    <p className="mutation-readonly-value">Pendiente</p>
                    <p className="mutation-field-help">
                      El backend asignará automáticamente este estado al crear la solicitud.
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
                disabled={isSubmitting || !!catalogError}
              >
                {isSubmitting ? (
                  <>
                    <span className="mutation-spinner" aria-hidden="true"></span>
                    <span>Guardando...</span>
                  </>
                ) : (
                  <>
                    <Save size={16} aria-hidden="true" />
                    <span>{mode === 'create' ? 'Guardar Solicitud' : 'Actualizar Solicitud'}</span>
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
