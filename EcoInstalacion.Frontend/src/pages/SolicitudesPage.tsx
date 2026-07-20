import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  AlertTriangle,
  CheckCircle,
  Eye,
  FileText,
  Pencil,
  Plus,
  RefreshCw,
  Trash2
} from 'lucide-react';
import { useAuth } from '../auth/useAuth';
import {
  canCreateSolicitud,
  canDeleteSolicitud,
  canEditSolicitud
} from '../utils/roleUtils';
import {
  createSolicitud,
  deleteSolicitud,
  getSolicitudById,
  getSolicitudes,
  updateSolicitud
} from '../features/solicitudes/solicitudesApi';
import type {
  SolicitudCreatePayload,
  SolicitudDetail,
  SolicitudEstado,
  SolicitudListItem,
  SolicitudUpdatePayload
} from '../features/solicitudes/types';
import { SOLICITUD_ESTADOS } from '../features/solicitudes/types';
import { SolicitudFormModal } from '../features/solicitudes/components/SolicitudFormModal';
import { ConfirmModal } from '../components/common/ConfirmModal';
import { HttpError } from '../services/httpClient';
import '../components/common/MutationUi.css';

type EstadoFiltro = 'Todas' | SolicitudEstado;
type FormMode = 'create' | 'edit' | 'view';

function estadoKey(estado?: string | null): string {
  return estado?.trim().toLocaleLowerCase('es') ?? '';
}

function getStatusClass(estado?: string | null): string {
  switch (estadoKey(estado)) {
    case 'pendiente':
      return 'badge-warning';
    case 'programada':
      return 'mutation-status-scheduled';
    case 'en proceso':
      return 'mutation-status-progress';
    case 'cerrada':
      return 'badge-success';
    case 'rechazada':
      return 'badge-danger';
    default:
      return 'badge-secondary';
  }
}

function formatDate(fecha?: string | null): string {
  if (!fecha) return 'N/A';

  const value = new Date(fecha);
  if (Number.isNaN(value.getTime())) return 'N/A';

  return value.toLocaleDateString('es-SV');
}

export default function SolicitudesPage() {
  const { user } = useAuth();

  const [solicitudes, setSolicitudes] = useState<SolicitudListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const requestIdRef = useRef<number>(0);
  const detailRequestIdRef = useRef<number>(0);

  const [estadoFiltro, setEstadoFiltro] = useState<EstadoFiltro>('Todas');
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [opError, setOpError] = useState<string | null>(null);

  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [formMode, setFormMode] = useState<FormMode>('create');
  const [selectedSolicitudDetail, setSelectedSolicitudDetail] =
    useState<SolicitudDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState<boolean>(false);

  const [isDeleteOpen, setIsDeleteOpen] = useState<boolean>(false);
  const [solicitudToDelete, setSolicitudToDelete] =
    useState<SolicitudListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const showCreateBtn = canCreateSolicitud(user);
  const showEditBtn = canEditSolicitud(user);
  const showDeleteBtn = canDeleteSolicitud(user);

  const solicitudesFiltradas = useMemo(() => {
    if (estadoFiltro === 'Todas') return solicitudes;
    return solicitudes.filter(
      (solicitud) => estadoKey(solicitud.estado) === estadoKey(estadoFiltro)
    );
  }, [estadoFiltro, solicitudes]);

  const fetchSolicitudesList = useCallback(async () => {
    const requestId = ++requestIdRef.current;
    setLoading(true);
    setError(null);
    setOpError(null);

    try {
      const data = await getSolicitudes();
      if (requestId !== requestIdRef.current) return;
      setSolicitudes(data);
    } catch (fetchError) {
      if (requestId !== requestIdRef.current) return;

      if (fetchError instanceof HttpError) {
        if (fetchError.status === 403) {
          setError('Acceso Denegado: Tu rol no puede consultar las solicitudes.');
        } else if (fetchError.status === 401) {
          setError(fetchError.message || 'Sesión no autorizada o expirada.');
        } else {
          setError(fetchError.message || 'Error al cargar la lista de solicitudes.');
        }
      } else if (fetchError instanceof Error) {
        setError(fetchError.message);
      } else {
        setError('Error de red al cargar las solicitudes de instalación.');
      }
    } finally {
      if (requestId === requestIdRef.current) setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchSolicitudesList();
  }, [fetchSolicitudesList]);

  const handleCloseFormModal = () => {
    detailRequestIdRef.current += 1;
    setIsFormOpen(false);
    setLoadingDetail(false);
  };

  const handleOpenCreateModal = () => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode('create');
    setSelectedSolicitudDetail(null);
    setIsFormOpen(true);
  };

  const handleOpenDetailModal = async (
    solicitud: SolicitudListItem,
    mode: Extract<FormMode, 'edit' | 'view'>
  ) => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode(mode);
    setSelectedSolicitudDetail(null);
    setIsFormOpen(true);
    setLoadingDetail(true);

    const requestId = ++detailRequestIdRef.current;

    try {
      const detail = await getSolicitudById(solicitud.id);
      if (requestId !== detailRequestIdRef.current) return;
      setSelectedSolicitudDetail(detail);
    } catch (detailError) {
      if (requestId !== detailRequestIdRef.current) return;
      setIsFormOpen(false);

      if (detailError instanceof HttpError) {
        if (detailError.status === 404) {
          await fetchSolicitudesList();
          setOpError(`La solicitud #${solicitud.id} ya no existe.`);
        } else if (detailError.status === 403) {
          setOpError('Acceso Denegado: No posees permisos para consultar esta solicitud.');
        } else if (detailError.status === 401) {
          setOpError(detailError.message || 'Sesión no autorizada o expirada.');
        } else {
          setOpError(detailError.message || 'Error al obtener el detalle de la solicitud.');
        }
      } else if (detailError instanceof Error) {
        setOpError(detailError.message);
      } else {
        setOpError('Error de conexión al cargar el detalle de la solicitud.');
      }
    } finally {
      if (requestId === detailRequestIdRef.current) setLoadingDetail(false);
    }
  };

  const handleSaveSolicitud = async (
    payload: SolicitudCreatePayload | SolicitudUpdatePayload
  ) => {
    if (formMode === 'create') {
      await createSolicitud(payload as SolicitudCreatePayload);
      setIsFormOpen(false);
      await fetchSolicitudesList();
      setSuccessMsg('Solicitud creada correctamente con estado Pendiente.');
      return;
    }

    if (formMode !== 'edit' || !selectedSolicitudDetail) return;

    try {
      await updateSolicitud(
        selectedSolicitudDetail.id,
        payload as SolicitudUpdatePayload
      );
      setIsFormOpen(false);
      await fetchSolicitudesList();
      setSuccessMsg(`Solicitud #${selectedSolicitudDetail.id} actualizada correctamente.`);
    } catch (saveError) {
      if (saveError instanceof HttpError && saveError.status === 404) {
        await fetchSolicitudesList();
      }
      throw saveError;
    }
  };

  const handleOpenDeleteModal = (solicitud: SolicitudListItem) => {
    setOpError(null);
    setSuccessMsg(null);
    setSolicitudToDelete(solicitud);
    setDeleteError(null);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!solicitudToDelete) return;

    setIsDeleting(true);
    setDeleteError(null);

    try {
      await deleteSolicitud(solicitudToDelete.id);
      const solicitudId = solicitudToDelete.id;
      setIsDeleteOpen(false);
      setSolicitudToDelete(null);
      await fetchSolicitudesList();
      setSuccessMsg(`Solicitud #${solicitudId} eliminada correctamente.`);
    } catch (deleteFailure) {
      if (deleteFailure instanceof HttpError) {
        if (deleteFailure.status === 400) {
          setDeleteError(
            deleteFailure.message ||
              'No se puede eliminar la solicitud porque tiene visitas técnicas asociadas.'
          );
        } else if (deleteFailure.status === 403) {
          setDeleteError('Acceso Denegado: No posees permisos para eliminar solicitudes.');
        } else if (deleteFailure.status === 404) {
          setIsDeleteOpen(false);
          setSolicitudToDelete(null);
          setDeleteError(null);
          await fetchSolicitudesList();
          setOpError('La solicitud ya no existe. Se actualizó el listado.');
        } else if (deleteFailure.status === 401) {
          setDeleteError(deleteFailure.message || 'Sesión no autorizada o expirada.');
        } else {
          setDeleteError(
            deleteFailure.message || 'Ocurrió un error al eliminar la solicitud.'
          );
        }
      } else if (deleteFailure instanceof Error) {
        setDeleteError(deleteFailure.message);
      } else {
        setDeleteError('Error de red al intentar eliminar la solicitud.');
      }
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div className="mutation-header-row">
          <h2 className="preview-title mutation-page-title" style={{ margin: 0 }}>
            <FileText
              size={22}
              aria-hidden="true"
              style={{ verticalAlign: 'middle', marginRight: 8 }}
            />
            Solicitudes de Instalación
          </h2>
          <div className="mutation-header-actions">
            <span className="count-tag" style={{ fontSize: '14px', padding: '6px 12px' }}>
              Total: {solicitudes.length}
            </span>
            {showCreateBtn && (
              <button
                type="button"
                className="mutation-btn mutation-btn-primary"
                onClick={handleOpenCreateModal}
              >
                <Plus size={16} aria-hidden="true" />
                <span>Nueva Solicitud</span>
              </button>
            )}
          </div>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: '16px' }}>
          <strong>Gestión de Solicitudes:</strong> Seguimiento de servicios eléctricos y
          solares desde su registro hasta el cierre.
        </p>
      </div>

      {successMsg && (
        <div className="mutation-success-banner" role="status" aria-live="polite">
          <CheckCircle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
          <span>{successMsg}</span>
        </div>
      )}

      {opError && (
        <div className="mutation-page-op-error" role="alert" aria-live="polite">
          <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
          <span>{opError}</span>
        </div>
      )}

      {!loading && !error && solicitudes.length > 0 && (
        <div className="mutation-filter-bar" aria-label="Filtrar solicitudes por estado">
          <span className="mutation-filter-label">Estado:</span>
          {(['Todas', ...SOLICITUD_ESTADOS] as EstadoFiltro[]).map((estado) => (
            <button
              key={estado}
              type="button"
              className={`mutation-filter-btn ${
                estadoFiltro === estado ? 'mutation-filter-btn--active' : ''
              }`}
              aria-pressed={estadoFiltro === estado}
              onClick={() => setEstadoFiltro(estado)}
            >
              {estado}
            </button>
          ))}
          <span className="mutation-filter-count">
            Mostrando {solicitudesFiltradas.length} de {solicitudes.length}
          </span>
        </div>
      )}

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de solicitudes...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">
            <AlertTriangle
              size={16}
              aria-hidden="true"
              style={{ verticalAlign: 'middle', marginRight: 6 }}
            />
            Error de Carga
          </p>
          <p className="error-desc">{error}</p>
          <button
            type="button"
            className="mutation-btn mutation-btn-secondary mutation-btn-sm mutation-retry-margin"
            onClick={() => void fetchSolicitudesList()}
          >
            <RefreshCw size={14} aria-hidden="true" />
            <span>Reintentar</span>
          </button>
        </div>
      )}

      {!loading && !error && (
        <>
          {solicitudes.length === 0 ? (
            <p className="empty-message">
              No se encontraron solicitudes registradas en el sistema.
            </p>
          ) : solicitudesFiltradas.length === 0 ? (
            <p className="empty-message">
              No existen solicitudes con el estado “{estadoFiltro}”.
            </p>
          ) : (
            <div className="table-responsive">
              <table className="entity-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Fecha Solicitud</th>
                    <th>Cliente</th>
                    <th>Tipo de Instalación</th>
                    <th>Técnico Asignado</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {solicitudesFiltradas.map((solicitud) => (
                    <tr key={solicitud.id}>
                      <td className="text-bold">#{solicitud.id}</td>
                      <td>{formatDate(solicitud.fechaSolicitud)}</td>
                      <td>
                        {solicitud.clienteNombre || `Cliente #${solicitud.clienteId}`}
                      </td>
                      <td>
                        <span className="service-type-tag">
                          {solicitud.tipoInstalacion || 'No especificado'}
                        </span>
                      </td>
                      <td>
                        {solicitud.tecnicoNombre || (
                          <span className="text-muted">No asignado</span>
                        )}
                      </td>
                      <td>
                        <span
                          className={`status-pill ${getStatusClass(solicitud.estado)}`}
                        >
                          {solicitud.estado || 'Sin estado'}
                        </span>
                      </td>
                      <td>
                        <div className="mutation-action-buttons">
                          <button
                            type="button"
                            className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                            onClick={() => void handleOpenDetailModal(solicitud, 'view')}
                            title="Ver detalle de la solicitud"
                          >
                            <Eye size={13} aria-hidden="true" />
                            <span>Ver</span>
                          </button>
                          {showEditBtn && (
                            <button
                              type="button"
                              className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                              onClick={() => void handleOpenDetailModal(solicitud, 'edit')}
                              title="Editar solicitud"
                            >
                              <Pencil size={13} aria-hidden="true" />
                              <span>Editar</span>
                            </button>
                          )}
                          {showDeleteBtn && (
                            <button
                              type="button"
                              className="mutation-btn mutation-btn-danger mutation-btn-sm"
                              onClick={() => handleOpenDeleteModal(solicitud)}
                              title="Eliminar solicitud"
                            >
                              <Trash2 size={13} aria-hidden="true" />
                              <span>Eliminar</span>
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <div className="note" style={{ marginTop: '24px' }}>
        {showEditBtn || showDeleteBtn ? (
          <>
            <strong>Administración:</strong> Puede registrar, consultar y actualizar
            solicitudes. La eliminación se bloqueará cuando existan visitas técnicas
            asociadas.
          </>
        ) : (
          <>
            <strong>Modo de Consulta:</strong> El rol Técnico puede revisar listados y
            detalles, sin permisos de creación, edición o eliminación.
          </>
        )}
      </div>

      <SolicitudFormModal
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedSolicitudDetail}
        isLoadingDetail={loadingDetail}
        onSave={handleSaveSolicitud}
        onClose={handleCloseFormModal}
      />

      <ConfirmModal
        isOpen={isDeleteOpen}
        title="Confirmar Eliminación"
        message={
          solicitudToDelete
            ? `¿Está seguro que desea eliminar la solicitud #${solicitudToDelete.id}? Esta acción no se puede deshacer y será bloqueada si existen visitas asociadas.`
            : '¿Está seguro que desea eliminar esta solicitud?'
        }
        confirmText="Sí, Eliminar"
        cancelText="Cancelar"
        confirmIcon={<Trash2 size={14} />}
        isSubmitting={isDeleting}
        error={deleteError}
        onConfirm={() => void handleConfirmDelete()}
        onCancel={() => {
          if (!isDeleting) {
            setIsDeleteOpen(false);
            setSolicitudToDelete(null);
            setDeleteError(null);
          }
        }}
      />
    </div>
  );
}
