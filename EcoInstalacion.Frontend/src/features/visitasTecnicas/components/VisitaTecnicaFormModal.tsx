import React, { useEffect, useId, useMemo, useRef, useState } from 'react';
import { AlertTriangle, Save, X } from 'lucide-react';
import { getSolicitudes } from '../../solicitudes/solicitudesApi';
import type { SolicitudListItem } from '../../solicitudes/types';
import { HttpError } from '../../../services/httpClient';
import type {
  VisitaTecnicaCreatePayload,
  VisitaTecnicaDetail,
  VisitaTecnicaUpdatePayload
} from '../types';
import '../../../components/common/MutationUi.css';

type VisitaTecnicaModalMode = 'create' | 'edit' | 'view';

interface VisitaTecnicaFormModalProps {
  isOpen: boolean;
  mode: VisitaTecnicaModalMode;
  initialData?: VisitaTecnicaDetail | null;
  isLoadingDetail?: boolean;
  onSave: (
    payload: VisitaTecnicaCreatePayload | VisitaTecnicaUpdatePayload
  ) => Promise<void>;
  onClose: () => void;
}

interface FormState {
  solicitudId: string;
  fechaVisita: string;
  observaciones: string;
}

interface ValidationErrors {
  solicitudId?: string;
  fechaVisita?: string;
  observaciones?: string;
}

const EMPTY_FORM: FormState = {
  solicitudId: '',
  fechaVisita: '',
  observaciones: ''
};

function estadoKey(estado?: string | null): string {
  return estado?.trim().toLocaleLowerCase('es') ?? '';
}

function solicitudDisponible(solicitud: SolicitudListItem): boolean {
  const estado = estadoKey(solicitud.estado);
  return estado === 'pendiente' || estado === 'programada' || estado === 'en proceso';
}

function toDateTimeLocalValue(fecha?: string | null): string {
  if (!fecha) return '';

  const withoutTimeZone = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(fecha) &&
    !/(?:Z|[+-]\d{2}:\d{2})$/i.test(fecha);
  if (withoutTimeZone) return fecha.slice(0, 16);

  const value = new Date(fecha);
  if (Number.isNaN(value.getTime())) return '';

  const pad = (part: number) => String(part).padStart(2, '0');
  return `${value.getFullYear()}-${pad(value.getMonth() + 1)}-${pad(value.getDate())}T${pad(value.getHours())}:${pad(value.getMinutes())}`;
}

function toApiDateTime(value: string): string {
  return value.length === 16 ? `${value}:00` : value;
}

function formatDateTime(fecha?: string | null): string {
  const localValue = toDateTimeLocalValue(fecha);
  if (!localValue) return 'No disponible';

  const value = new Date(localValue);
  if (Number.isNaN(value.getTime())) return 'No disponible';

  return value.toLocaleString('es-SV', {
    dateStyle: 'medium',
    timeStyle: 'short'
  });
}

export const VisitaTecnicaFormModal: React.FC<VisitaTecnicaFormModalProps> = ({
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
  const solicitudRef = useRef<HTMLSelectElement>(null);
  const fechaRef = useRef<HTMLInputElement>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  const catalogRequestIdRef = useRef<number>(0);

  const [form, setForm] = useState<FormState>(EMPTY_FORM);
  const [solicitudes, setSolicitudes] = useState<SolicitudListItem[]>([]);
  const [loadingCatalog, setLoadingCatalog] = useState<boolean>(false);
  const [catalogError, setCatalogError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<ValidationErrors>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [serverFieldErrors, setServerFieldErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  const isViewMode = mode === 'view';
  const solicitudesDisponibles = useMemo(
    () => solicitudes.filter(solicitudDisponible),
    [solicitudes]
  );

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
        solicitudId: String(initialData.solicitudId),
        fechaVisita: toDateTimeLocalValue(initialData.fechaVisita),
        observaciones: initialData.observaciones || ''
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
    if (!isOpen || mode !== 'create') return;

    const requestId = ++catalogRequestIdRef.current;
    setLoadingCatalog(true);
    setCatalogError(null);

    void getSolicitudes()
      .then((data) => {
        if (requestId !== catalogRequestIdRef.current) return;
        setSolicitudes(data);
      })
      .catch((error: unknown) => {
        if (requestId !== catalogRequestIdRef.current) return;

        if (error instanceof HttpError) {
          setCatalogError(error.message || 'No fue posible cargar las solicitudes.');
        } else if (error instanceof Error) {
          setCatalogError(error.message);
        } else {
          setCatalogError('No fue posible cargar las solicitudes.');
        }
      })
      .finally(() => {
        if (requestId === catalogRequestIdRef.current) setLoadingCatalog(false);
      });

    return () => {
      catalogRequestIdRef.current += 1;
    };
  }, [isOpen, mode]);

  useEffect(() => {
    if (!isOpen) return;

    requestAnimationFrame(() => {
      if (isLoadingDetail || loadingCatalog || isViewMode) {
        dialogRef.current?.focus();
      } else if (mode === 'create') {
        solicitudRef.current?.focus();
      } else {
        fechaRef.current?.focus();
      }
    });
  }, [isOpen, isLoadingDetail, loadingCatalog, isViewMode, mode]);

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
    const solicitudId = Number(form.solicitudId);
    const observaciones = form.observaciones.trim();

    if (mode === 'create') {
      if (!Number.isInteger(solicitudId) || solicitudId <= 0) {
        errors.solicitudId = 'Debe seleccionar una solicitud válida.';
      } else if (!solicitudesDisponibles.some((item) => item.id === solicitudId)) {
        errors.solicitudId = 'La solicitud seleccionada ya no está disponible.';
      }
    }

    if (!form.fechaVisita) {
      errors.fechaVisita = 'La fecha y hora de visita son obligatorias.';
    } else if (Number.isNaN(new Date(form.fechaVisita).getTime())) {
      errors.fechaVisita = 'La fecha de visita no es válida.';
    } else if (new Date(form.fechaVisita) < new Date('1753-01-01T00:00')) {
      errors.fechaVisita = 'La fecha de visita no puede ser anterior al año 1753.';
    }

    if (observaciones.length > 500) {
      errors.observaciones = 'Las observaciones no deben exceder 500 caracteres.';
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

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setServerError(null);
    setServerFieldErrors([]);

    if (!validate()) return;

    const basePayload: VisitaTecnicaUpdatePayload = {
      fechaVisita: toApiDateTime(form.fechaVisita),
      observaciones: form.observaciones.trim() || null,
      aprobado: initialData?.aprobado === true
    };

    const payload: VisitaTecnicaCreatePayload | VisitaTecnicaUpdatePayload =
      mode === 'create'
        ? {
            ...basePayload,
            solicitudId: Number(form.solicitudId),
            aprobado: false
          }
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
          setServerError(error.message || 'Error de validación en la visita técnica.');
        } else if (error.status === 403) {
          setServerError('Acceso Denegado: No posees permisos para administrar visitas.');
        } else if (error.status === 404) {
          setServerError('La visita o la solicitud asociada ya no existe.');
        } else if (error.status === 401) {
          setServerError(error.message || 'Sesión no autorizada o expirada.');
        } else {
          setServerError(error.message || 'Ocurrió un error al procesar la visita.');
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

  const isLoading = isLoadingDetail || (mode === 'create' && loadingCatalog);
  const noSolicitudesDisponibles =
    mode === 'create' && !loadingCatalog && !catalogError && solicitudesDisponibles.length === 0;
  const hasServerError = serverError !== null;
  const modalTitle =
    mode === 'create'
      ? 'Nueva Visita Técnica'
      : mode === 'edit'
        ? 'Editar Visita Técnica'
        : 'Detalle de Visita Técnica';

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
                ? 'Cargando detalle de la visita...'
                : 'Cargando solicitudes disponibles...'}
            </p>
          </div>
        ) : isViewMode ? (
          <>
            <div className="mutation-modal-body">
              {initialData ? (
                <dl className="mutation-detail-grid">
                  <div className="mutation-detail-item">
                    <dt>Visita</dt>
                    <dd>#{initialData.id}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Fecha y hora</dt>
                    <dd>{formatDateTime(initialData.fechaVisita)}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Solicitud</dt>
                    <dd>#{initialData.solicitudId}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Aprobación</dt>
                    <dd>{initialData.aprobado ? 'Aprobada' : 'Pendiente'}</dd>
                  </div>
                  <div className="mutation-detail-item">
                    <dt>Cliente</dt>
                    <dd>{initialData.clienteNombre || 'No disponible'}</dd>
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
                    <dt>Estado de solicitud</dt>
                    <dd>{initialData.estadoSolicitud || 'No especificado'}</dd>
                  </div>
                  <div className="mutation-detail-item mutation-detail-item-wide">
                    <dt>Observaciones</dt>
                    <dd>{initialData.observaciones || 'Sin observaciones registradas.'}</dd>
                  </div>
                </dl>
              ) : (
                <div className="mutation-error-banner" role="alert">
                  <AlertTriangle size={15} aria-hidden="true" />
                  <span>No fue posible mostrar el detalle de la visita.</span>
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
              {(hasServerError || catalogError || noSolicitudesDisponibles) && (
                <div
                  id={serverErrorId}
                  className="mutation-error-banner"
                  role="alert"
                  aria-live="assertive"
                >
                  <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
                  <div>
                    <strong>Error:</strong>{' '}
                    {serverError ||
                      catalogError ||
                      'No existen solicitudes abiertas disponibles para registrar una visita.'}
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
                {mode === 'create' ? (
                  <div className="mutation-form-group mutation-form-group-wide">
                    <label htmlFor={`${uid}-solicitudId`} className="mutation-form-label">
                      Solicitud <span className="required-star" aria-hidden="true">*</span>
                    </label>
                    <select
                      ref={solicitudRef}
                      id={`${uid}-solicitudId`}
                      name="solicitudId"
                      className={`mutation-form-control ${validationErrors.solicitudId ? 'is-invalid' : ''}`}
                      value={form.solicitudId}
                      onChange={handleChange}
                      disabled={isSubmitting || !!catalogError || noSolicitudesDisponibles}
                      required
                      aria-required="true"
                      aria-invalid={!!validationErrors.solicitudId}
                      aria-describedby={
                        validationErrors.solicitudId
                          ? fieldErrorId('solicitudId')
                          : hasServerError || catalogError || noSolicitudesDisponibles
                            ? serverErrorId
                            : undefined
                      }
                    >
                      <option value="">Seleccione una solicitud abierta</option>
                      {solicitudesDisponibles.map((solicitud) => (
                        <option key={solicitud.id} value={solicitud.id}>
                          #{solicitud.id} — {solicitud.clienteNombre || `Cliente #${solicitud.clienteId}`} — {solicitud.tipoInstalacion || 'Sin tipo'} ({solicitud.estado})
                        </option>
                      ))}
                    </select>
                    {validationErrors.solicitudId && (
                      <p
                        id={fieldErrorId('solicitudId')}
                        className="mutation-field-error"
                        role="alert"
                      >
                        {validationErrors.solicitudId}
                      </p>
                    )}
                  </div>
                ) : (
                  <div className="mutation-form-group mutation-form-group-wide">
                    <span className="mutation-form-label">Solicitud asociada</span>
                    <p className="mutation-readonly-value">
                      #{initialData?.solicitudId} — {initialData?.clienteNombre || 'Cliente no disponible'}
                    </p>
                    <p className="mutation-field-help">
                      La solicitud asociada no puede modificarse después de crear la visita.
                    </p>
                  </div>
                )}

                <div className="mutation-form-group">
                  <label htmlFor={`${uid}-fechaVisita`} className="mutation-form-label">
                    Fecha y hora <span className="required-star" aria-hidden="true">*</span>
                  </label>
                  <input
                    ref={fechaRef}
                    id={`${uid}-fechaVisita`}
                    name="fechaVisita"
                    type="datetime-local"
                    min="1753-01-01T00:00"
                    className={`mutation-form-control ${validationErrors.fechaVisita ? 'is-invalid' : ''}`}
                    value={form.fechaVisita}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    required
                    aria-required="true"
                    aria-invalid={!!validationErrors.fechaVisita}
                    aria-describedby={
                      validationErrors.fechaVisita
                        ? fieldErrorId('fechaVisita')
                        : `${uid}-fecha-help`
                    }
                  />
                  <p id={`${uid}-fecha-help`} className="mutation-field-help">
                    La fecha se conservará en la hora local indicada.
                  </p>
                  {validationErrors.fechaVisita && (
                    <p
                      id={fieldErrorId('fechaVisita')}
                      className="mutation-field-error"
                      role="alert"
                    >
                      {validationErrors.fechaVisita}
                    </p>
                  )}
                </div>

                <div className="mutation-form-group">
                  <span className="mutation-form-label">Estado de aprobación</span>
                  <p className="mutation-readonly-value">
                    {mode === 'create'
                      ? 'Pendiente'
                      : initialData?.aprobado
                        ? 'Aprobada — estado irreversible'
                        : 'Pendiente'}
                  </p>
                  <p className="mutation-field-help">
                    La aprobación se realiza mediante una acción independiente y confirmada.
                  </p>
                </div>

                <div className="mutation-form-group mutation-form-group-wide">
                  <label htmlFor={`${uid}-observaciones`} className="mutation-form-label">
                    Observaciones
                  </label>
                  <textarea
                    id={`${uid}-observaciones`}
                    name="observaciones"
                    rows={5}
                    className={`mutation-form-control ${validationErrors.observaciones ? 'is-invalid' : ''}`}
                    maxLength={500}
                    value={form.observaciones}
                    onChange={handleChange}
                    disabled={isSubmitting}
                    placeholder="Hallazgos, mediciones y recomendaciones de la visita."
                    aria-invalid={!!validationErrors.observaciones}
                    aria-describedby={
                      validationErrors.observaciones
                        ? fieldErrorId('observaciones')
                        : `${uid}-observaciones-help`
                    }
                  />
                  <p id={`${uid}-observaciones-help`} className="mutation-field-help">
                    {form.observaciones.length}/500 caracteres
                  </p>
                  {validationErrors.observaciones && (
                    <p
                      id={fieldErrorId('observaciones')}
                      className="mutation-field-error"
                      role="alert"
                    >
                      {validationErrors.observaciones}
                    </p>
                  )}
                </div>
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
                disabled={
                  isSubmitting ||
                  !!catalogError ||
                  noSolicitudesDisponibles ||
                  (mode === 'edit' && !initialData)
                }
              >
                {isSubmitting ? (
                  <>
                    <span className="mutation-spinner" aria-hidden="true"></span>
                    <span>Guardando...</span>
                  </>
                ) : (
                  <>
                    <Save size={16} aria-hidden="true" />
                    <span>{mode === 'create' ? 'Guardar Visita' : 'Actualizar Visita'}</span>
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
