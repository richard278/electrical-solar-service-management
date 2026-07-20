import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
  AlertTriangle,
  CalendarCheck2,
  CheckCircle,
  Eye,
  Pencil,
  Plus,
  RefreshCw,
  ShieldCheck,
  Trash2
} from 'lucide-react';
import { useAuth } from '../auth/useAuth';
import {
  canApproveVisitaTecnica,
  canCreateVisitaTecnica,
  canDeleteVisitaTecnica,
  canEditVisitaTecnica
} from '../utils/roleUtils';
import {
  createVisitaTecnica,
  deleteVisitaTecnica,
  getVisitaTecnicaById,
  getVisitasTecnicas,
  updateVisitaTecnica
} from '../features/visitasTecnicas/visitasTecnicasApi';
import type {
  VisitaTecnicaCreatePayload,
  VisitaTecnicaDetail,
  VisitaTecnicaListItem,
  VisitaTecnicaUpdatePayload
} from '../features/visitasTecnicas/types';
import { VisitaTecnicaFormModal } from '../features/visitasTecnicas/components/VisitaTecnicaFormModal';
import { ConfirmModal } from '../components/common/ConfirmModal';
import { HttpError } from '../services/httpClient';
import '../components/common/MutationUi.css';

type AprobacionFiltro = 'Todas' | 'Pendientes' | 'Aprobadas';
type FormMode = 'create' | 'edit' | 'view';

function estadoKey(estado?: string | null): string {
  return estado?.trim().toLocaleLowerCase('es') ?? '';
}

function getSolicitudStatusClass(estado?: string | null): string {
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

function formatDateTime(fecha?: string | null): string {
  if (!fecha) return 'N/A';

  const value = new Date(fecha);
  if (Number.isNaN(value.getTime())) return 'N/A';

  return value.toLocaleString('es-SV', {
    dateStyle: 'short',
    timeStyle: 'short'
  });
}

function isHistoricalMismatch(visita: VisitaTecnicaListItem): boolean {
  if (!visita.aprobado) return false;
  const estado = estadoKey(visita.estadoSolicitud);
  return estado === 'pendiente' || estado === 'programada';
}

function solicitudAllowsApproval(estado?: string | null): boolean {
  const estadoNormalizado = estadoKey(estado);
  return (
    estadoNormalizado === 'pendiente' ||
    estadoNormalizado === 'programada' ||
    estadoNormalizado === 'en proceso'
  );
}

export default function VisitasTecnicasPage() {
  const { user } = useAuth();

  const [visitas, setVisitas] = useState<VisitaTecnicaListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const requestIdRef = useRef<number>(0);
  const detailRequestIdRef = useRef<number>(0);

  const [aprobacionFiltro, setAprobacionFiltro] =
    useState<AprobacionFiltro>('Todas');
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [opError, setOpError] = useState<string | null>(null);

  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [formMode, setFormMode] = useState<FormMode>('create');
  const [selectedVisitaDetail, setSelectedVisitaDetail] =
    useState<VisitaTecnicaDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState<boolean>(false);

  const [isDeleteOpen, setIsDeleteOpen] = useState<boolean>(false);
  const [visitaToDelete, setVisitaToDelete] =
    useState<VisitaTecnicaListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const [isApproveOpen, setIsApproveOpen] = useState<boolean>(false);
  const [visitaToApprove, setVisitaToApprove] =
    useState<VisitaTecnicaListItem | null>(null);
  const [isApproving, setIsApproving] = useState<boolean>(false);
  const [approveError, setApproveError] = useState<string | null>(null);

  const showCreateBtn = canCreateVisitaTecnica(user);
  const showEditBtn = canEditVisitaTecnica(user);
  const showApproveBtn = canApproveVisitaTecnica(user);
  const showDeleteBtn = canDeleteVisitaTecnica(user);

  const visitasFiltradas = useMemo(() => {
    if (aprobacionFiltro === 'Todas') return visitas;
    const aprobadas = aprobacionFiltro === 'Aprobadas';
    return visitas.filter((visita) => visita.aprobado === aprobadas);
  }, [aprobacionFiltro, visitas]);

  const aprobadas = useMemo(
    () => visitas.filter((visita) => visita.aprobado).length,
    [visitas]
  );
  const pendientes = visitas.length - aprobadas;
  const inconsistenciasHistoricas = useMemo(
    () => visitas.filter(isHistoricalMismatch).length,
    [visitas]
  );

  const fetchVisitasList = useCallback(async () => {
    const requestId = ++requestIdRef.current;
    setLoading(true);
    setError(null);
    setOpError(null);

    try {
      const data = await getVisitasTecnicas();
      if (requestId !== requestIdRef.current) return;
      setVisitas(data);
    } catch (fetchError) {
      if (requestId !== requestIdRef.current) return;

      if (fetchError instanceof HttpError) {
        if (fetchError.status === 403) {
          setError('Acceso Denegado: Tu rol no puede consultar las visitas técnicas.');
        } else if (fetchError.status === 401) {
          setError(fetchError.message || 'Sesión no autorizada o expirada.');
        } else {
          setError(fetchError.message || 'Error al cargar las visitas técnicas.');
        }
      } else if (fetchError instanceof Error) {
        setError(fetchError.message);
      } else {
        setError('Error de red al cargar el registro de visitas técnicas.');
      }
    } finally {
      if (requestId === requestIdRef.current) setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchVisitasList();
  }, [fetchVisitasList]);

  const handleCloseFormModal = () => {
    detailRequestIdRef.current += 1;
    setIsFormOpen(false);
    setLoadingDetail(false);
  };

  const handleOpenCreateModal = () => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode('create');
    setSelectedVisitaDetail(null);
    setIsFormOpen(true);
  };

  const handleOpenDetailModal = async (
    visita: VisitaTecnicaListItem,
    mode: Extract<FormMode, 'edit' | 'view'>
  ) => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode(mode);
    setSelectedVisitaDetail(null);
    setIsFormOpen(true);
    setLoadingDetail(true);

    const requestId = ++detailRequestIdRef.current;

    try {
      const detail = await getVisitaTecnicaById(visita.id);
      if (requestId !== detailRequestIdRef.current) return;
      setSelectedVisitaDetail(detail);
    } catch (detailError) {
      if (requestId !== detailRequestIdRef.current) return;
      setIsFormOpen(false);

      if (detailError instanceof HttpError) {
        if (detailError.status === 404) {
          await fetchVisitasList();
          setOpError(`La visita #${visita.id} ya no existe.`);
        } else if (detailError.status === 403) {
          setOpError('Acceso Denegado: No posees permisos para consultar esta visita.');
        } else if (detailError.status === 401) {
          setOpError(detailError.message || 'Sesión no autorizada o expirada.');
        } else {
          setOpError(detailError.message || 'Error al obtener el detalle de la visita.');
        }
      } else if (detailError instanceof Error) {
        setOpError(detailError.message);
      } else {
        setOpError('Error de conexión al cargar el detalle de la visita.');
      }
    } finally {
      if (requestId === detailRequestIdRef.current) setLoadingDetail(false);
    }
  };

  const handleSaveVisita = async (
    payload: VisitaTecnicaCreatePayload | VisitaTecnicaUpdatePayload
  ) => {
    if (formMode === 'create') {
      await createVisitaTecnica(payload as VisitaTecnicaCreatePayload);
      setIsFormOpen(false);
      await fetchVisitasList();
      setSuccessMsg('Visita técnica creada correctamente con aprobación pendiente.');
      return;
    }

    if (formMode !== 'edit' || !selectedVisitaDetail) return;

    try {
      await updateVisitaTecnica(
        selectedVisitaDetail.id,
        payload as VisitaTecnicaUpdatePayload
      );
      setIsFormOpen(false);
      await fetchVisitasList();
      setSuccessMsg(`Visita #${selectedVisitaDetail.id} actualizada correctamente.`);
    } catch (saveError) {
      if (saveError instanceof HttpError && saveError.status === 404) {
        await fetchVisitasList();
      }
      throw saveError;
    }
  };

  const handleOpenApproveModal = (visita: VisitaTecnicaListItem) => {
    if (visita.aprobado) return;
    setOpError(null);
    setSuccessMsg(null);
    setVisitaToApprove(visita);
    setApproveError(null);
    setIsApproveOpen(true);
  };

  const handleConfirmApprove = async () => {
    if (!visitaToApprove) return;

    setIsApproving(true);
    setApproveError(null);

    try {
      const visitaActual = await getVisitaTecnicaById(visitaToApprove.id);

      if (visitaActual.aprobado) {
        const visitaId = visitaActual.id;
        setIsApproveOpen(false);
        setVisitaToApprove(null);
        await fetchVisitasList();
        setSuccessMsg(`La visita #${visitaId} ya se encontraba aprobada.`);
        return;
      }

      await updateVisitaTecnica(visitaActual.id, {
        fechaVisita: visitaActual.fechaVisita,
        observaciones: visitaActual.observaciones?.trim() || null,
        aprobado: true
      });
      const visitaId = visitaActual.id;
      setIsApproveOpen(false);
      setVisitaToApprove(null);
      await fetchVisitasList();
      setSuccessMsg(
        `Visita #${visitaId} aprobada. La solicitud asociada avanzó a En proceso.`
      );
    } catch (approveFailure) {
      if (approveFailure instanceof HttpError) {
        if (approveFailure.status === 400) {
          setApproveError(
            approveFailure.message || 'No se pudo aprobar la visita técnica.'
          );
        } else if (approveFailure.status === 403) {
          setApproveError('Acceso Denegado: No posees permisos para aprobar visitas.');
        } else if (approveFailure.status === 404) {
          setIsApproveOpen(false);
          setVisitaToApprove(null);
          setApproveError(null);
          await fetchVisitasList();
          setOpError('La visita ya no existe. Se actualizó el listado.');
        } else if (approveFailure.status === 401) {
          setApproveError(approveFailure.message || 'Sesión no autorizada o expirada.');
        } else {
          setApproveError(
            approveFailure.message || 'Ocurrió un error al aprobar la visita.'
          );
        }
      } else if (approveFailure instanceof Error) {
        setApproveError(approveFailure.message);
      } else {
        setApproveError('Error de red al intentar aprobar la visita.');
      }
    } finally {
      setIsApproving(false);
    }
  };

  const handleOpenDeleteModal = (visita: VisitaTecnicaListItem) => {
    if (visita.aprobado) return;
    setOpError(null);
    setSuccessMsg(null);
    setVisitaToDelete(visita);
    setDeleteError(null);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!visitaToDelete) return;

    setIsDeleting(true);
    setDeleteError(null);

    try {
      await deleteVisitaTecnica(visitaToDelete.id);
      const visitaId = visitaToDelete.id;
      setIsDeleteOpen(false);
      setVisitaToDelete(null);
      await fetchVisitasList();
      setSuccessMsg(`Visita #${visitaId} eliminada correctamente.`);
    } catch (deleteFailure) {
      if (deleteFailure instanceof HttpError) {
        if (deleteFailure.status === 400) {
          setDeleteError(
            deleteFailure.message || 'No se puede eliminar una visita aprobada.'
          );
        } else if (deleteFailure.status === 403) {
          setDeleteError('Acceso Denegado: No posees permisos para eliminar visitas.');
        } else if (deleteFailure.status === 404) {
          setIsDeleteOpen(false);
          setVisitaToDelete(null);
          setDeleteError(null);
          await fetchVisitasList();
          setOpError('La visita ya no existe. Se actualizó el listado.');
        } else if (deleteFailure.status === 401) {
          setDeleteError(deleteFailure.message || 'Sesión no autorizada o expirada.');
        } else {
          setDeleteError(
            deleteFailure.message || 'Ocurrió un error al eliminar la visita.'
          );
        }
      } else if (deleteFailure instanceof Error) {
        setDeleteError(deleteFailure.message);
      } else {
        setDeleteError('Error de red al intentar eliminar la visita.');
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
            <CalendarCheck2
              size={22}
              aria-hidden="true"
              style={{ verticalAlign: 'middle', marginRight: 8 }}
            />
            Visitas Técnicas
          </h2>
          <div className="mutation-header-actions">
            <span className="count-tag" style={{ fontSize: '14px', padding: '6px 12px' }}>
              Total: {visitas.length}
            </span>
            {showCreateBtn && (
              <button
                type="button"
                className="mutation-btn mutation-btn-primary"
                onClick={handleOpenCreateModal}
              >
                <Plus size={16} aria-hidden="true" />
                <span>Nueva Visita</span>
              </button>
            )}
          </div>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: '16px' }}>
          <strong>Gestión de Visitas:</strong> Inspecciones en sitio, hallazgos técnicos y
          aprobación controlada de cada visita.
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

      {!loading && !error && inconsistenciasHistoricas > 0 && (
        <div className="mutation-page-warning" role="status">
          <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
          <span>
            Se detectaron {inconsistenciasHistoricas} visita(s) aprobada(s) cuyo estado de
            solicitud aún es Pendiente o Programada. Son registros históricos previos a
            B4.2.0 y se muestran sin modificarlos.
          </span>
        </div>
      )}

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de visitas...</p>
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
            onClick={() => void fetchVisitasList()}
          >
            <RefreshCw size={14} aria-hidden="true" />
            <span>Reintentar</span>
          </button>
        </div>
      )}

      {!loading && !error && (
        <>
          <div className="metrics-grid" style={{ marginBottom: '24px' }}>
            <div className="metric-card">
              <span className="metric-label">Total Visitas</span>
              <span className="metric-value">{visitas.length}</span>
            </div>
            <div className="metric-card border-green">
              <span className="metric-label">Visitas Aprobadas</span>
              <span className="metric-value text-green">{aprobadas}</span>
            </div>
            <div className="metric-card border-amber">
              <span className="metric-label">Visitas Pendientes</span>
              <span className="metric-value text-amber">{pendientes}</span>
            </div>
          </div>

          {visitas.length > 0 && (
            <div className="mutation-filter-bar" aria-label="Filtrar visitas por aprobación">
              <span className="mutation-filter-label">Aprobación:</span>
              {(['Todas', 'Pendientes', 'Aprobadas'] as AprobacionFiltro[]).map(
                (filtro) => (
                  <button
                    key={filtro}
                    type="button"
                    className={`mutation-filter-btn ${
                      aprobacionFiltro === filtro ? 'mutation-filter-btn--active' : ''
                    }`}
                    aria-pressed={aprobacionFiltro === filtro}
                    onClick={() => setAprobacionFiltro(filtro)}
                  >
                    {filtro}
                  </button>
                )
              )}
              <span className="mutation-filter-count">
                Mostrando {visitasFiltradas.length} de {visitas.length}
              </span>
            </div>
          )}

          {visitas.length === 0 ? (
            <p className="empty-message">No se encontraron visitas técnicas registradas.</p>
          ) : visitasFiltradas.length === 0 ? (
            <p className="empty-message">
              No existen visitas con el filtro “{aprobacionFiltro}”.
            </p>
          ) : (
            <div className="table-responsive">
              <table className="entity-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Fecha y hora</th>
                    <th>Solicitud</th>
                    <th>Cliente</th>
                    <th>Tipo</th>
                    <th>Técnico</th>
                    <th>Aprobación</th>
                    <th>Estado solicitud</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {visitasFiltradas.map((visita) => (
                    <tr key={visita.id}>
                      <td className="text-bold">#{visita.id}</td>
                      <td>{formatDateTime(visita.fechaVisita)}</td>
                      <td>#{visita.solicitudId}</td>
                      <td>{visita.clienteNombre || 'No disponible'}</td>
                      <td>
                        <span className="service-type-tag">
                          {visita.tipoInstalacion || 'No especificado'}
                        </span>
                      </td>
                      <td>
                        {visita.tecnicoNombre || (
                          <span className="text-muted">No asignado</span>
                        )}
                      </td>
                      <td>
                        <span
                          className={`status-pill ${
                            visita.aprobado ? 'badge-success' : 'badge-warning'
                          }`}
                        >
                          {visita.aprobado ? 'Aprobada' : 'Pendiente'}
                        </span>
                      </td>
                      <td>
                        <span
                          className={`status-pill ${getSolicitudStatusClass(
                            visita.estadoSolicitud
                          )}`}
                        >
                          {visita.estadoSolicitud || 'Sin estado'}
                        </span>
                      </td>
                      <td>
                        <div className="mutation-action-buttons">
                          <button
                            type="button"
                            className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                            onClick={() => void handleOpenDetailModal(visita, 'view')}
                            title="Ver detalle de la visita"
                          >
                            <Eye size={13} aria-hidden="true" />
                            <span>Ver</span>
                          </button>
                          {showEditBtn && (
                            <button
                              type="button"
                              className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                              onClick={() => void handleOpenDetailModal(visita, 'edit')}
                              title="Editar fecha y observaciones"
                            >
                              <Pencil size={13} aria-hidden="true" />
                              <span>Editar</span>
                            </button>
                          )}
                          {showApproveBtn &&
                            !visita.aprobado &&
                            solicitudAllowsApproval(visita.estadoSolicitud) && (
                            <button
                              type="button"
                              className="mutation-btn mutation-btn-primary mutation-btn-sm"
                              onClick={() => handleOpenApproveModal(visita)}
                              title="Aprobar visita de forma irreversible"
                            >
                              <ShieldCheck size={13} aria-hidden="true" />
                              <span>Aprobar</span>
                            </button>
                          )}
                          {showDeleteBtn && !visita.aprobado && (
                            <button
                              type="button"
                              className="mutation-btn mutation-btn-danger mutation-btn-sm"
                              onClick={() => handleOpenDeleteModal(visita)}
                              title="Eliminar visita pendiente"
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
        {showEditBtn || showApproveBtn || showDeleteBtn ? (
          <>
            <strong>Administración:</strong> Las visitas nuevas nacen pendientes. La
            aprobación es irreversible, avanza la solicitud a “En proceso” y protege la
            visita contra eliminación.
          </>
        ) : (
          <>
            <strong>Modo de Consulta:</strong> El rol Técnico puede revisar listados y
            detalles, sin permisos de creación, edición, aprobación o eliminación.
          </>
        )}
      </div>

      <VisitaTecnicaFormModal
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedVisitaDetail}
        isLoadingDetail={loadingDetail}
        onSave={handleSaveVisita}
        onClose={handleCloseFormModal}
      />

      <ConfirmModal
        isOpen={isApproveOpen}
        title="Confirmar Aprobación"
        message={
          visitaToApprove
            ? `¿Desea aprobar la visita #${visitaToApprove.id} de la solicitud #${visitaToApprove.solicitudId}? La solicitud avanzará a “En proceso” y esta acción no podrá revertirse.`
            : '¿Desea aprobar esta visita técnica?'
        }
        confirmText="Sí, Aprobar"
        cancelText="Cancelar"
        confirmVariant="primary"
        submittingText="Aprobando..."
        confirmIcon={<ShieldCheck size={14} />}
        isSubmitting={isApproving}
        error={approveError}
        onConfirm={() => void handleConfirmApprove()}
        onCancel={() => {
          if (!isApproving) {
            setIsApproveOpen(false);
            setVisitaToApprove(null);
            setApproveError(null);
          }
        }}
      />

      <ConfirmModal
        isOpen={isDeleteOpen}
        title="Confirmar Eliminación"
        message={
          visitaToDelete
            ? `¿Está seguro que desea eliminar la visita pendiente #${visitaToDelete.id}? Esta acción no se puede deshacer.`
            : '¿Está seguro que desea eliminar esta visita?'
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
            setVisitaToDelete(null);
            setDeleteError(null);
          }
        }}
      />
    </div>
  );
}
