import { useCallback, useEffect, useRef, useState } from 'react';
import {
  AlertTriangle,
  CheckCircle,
  ChevronLeft,
  ChevronRight,
  Filter,
  Pencil,
  Plus,
  RefreshCw,
  Search,
  Trash2,
  Wrench
} from 'lucide-react';
import { useAuth } from '../auth/useAuth';
import {
  canCreateTecnico,
  canDeleteTecnico,
  canEditTecnico
} from '../utils/roleUtils';
import {
  createTecnico,
  deleteTecnico,
  getTecnicoById,
  getTecnicosPaginados,
  updateTecnico
} from '../features/tecnicos/tecnicosApi';
import type {
  TecnicoCreatePayload,
  TecnicoDetail,
  TecnicoEstadoFiltro,
  TecnicoListItem,
  TecnicoPagedQuery,
  TecnicoUpdatePayload
} from '../features/tecnicos/types';
import { TecnicoFormModal } from '../features/tecnicos/components/TecnicoFormModal';
import { ConfirmModal } from '../components/common/ConfirmModal';
import { HttpError } from '../services/httpClient';
import '../components/common/MutationUi.css';

export default function TecnicosPage() {
  const { user } = useAuth();

  const [tecnicos, setTecnicos] = useState<TecnicoListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const requestIdRef = useRef<number>(0);
  const detailRequestIdRef = useRef<number>(0);

  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [opError, setOpError] = useState<string | null>(null);

  // Pagination & Filter state
  const [q, setQ] = useState<string>('');
  const [filtro, setFiltro] = useState<TecnicoEstadoFiltro>('activos');
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [totalPages, setTotalPages] = useState<number>(0);
  const [hasPreviousPage, setHasPreviousPage] = useState<boolean>(false);
  const [hasNextPage, setHasNextPage] = useState<boolean>(false);

  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [formMode, setFormMode] = useState<'create' | 'edit'>('create');
  const [selectedTecnicoDetail, setSelectedTecnicoDetail] = useState<TecnicoDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState<boolean>(false);

  const [isDeleteOpen, setIsDeleteOpen] = useState<boolean>(false);
  const [tecnicoToDelete, setTecnicoToDelete] = useState<TecnicoListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const showCreateBtn = canCreateTecnico(user);
  const showEditBtn = canEditTecnico(user);
  const showDeleteBtn = canDeleteTecnico(user);
  const showActionsCol = showEditBtn || showDeleteBtn;

  // ─── Fetch Paginado ───────────────────────────────────────────────

  const fetchTecnicosList = useCallback(async (query: TecnicoPagedQuery) => {
    const requestId = ++requestIdRef.current;
    setLoading(true);
    setError(null);
    setOpError(null);

    try {
      const data = await getTecnicosPaginados(query);
      if (requestId !== requestIdRef.current) return;
      setTecnicos(data.items || []);
      setTotalCount(data.totalCount || 0);
      setTotalPages(data.totalPages || 0);
      setHasPreviousPage(!!data.hasPreviousPage);
      setHasNextPage(!!data.hasNextPage);
    } catch (fetchError) {
      if (requestId !== requestIdRef.current) return;

      if (fetchError instanceof HttpError) {
        if (fetchError.status === 403) {
          setError('Acceso Denegado: Tu rol no puede consultar el catálogo de técnicos.');
        } else if (fetchError.status === 401) {
          setError(fetchError.message || 'Sesión no autorizada o expirada.');
        } else {
          setError(fetchError.message || 'Error al cargar la lista de técnicos.');
        }
      } else if (fetchError instanceof Error) {
        setError(fetchError.message);
      } else {
        setError('Error de red al cargar el catálogo de técnicos.');
      }
    } finally {
      if (requestId === requestIdRef.current) setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchTecnicosList({
      q,
      estado: filtro,
      pageNumber,
      pageSize
    });
  }, [fetchTecnicosList, q, filtro, pageNumber, pageSize]);

  // ─── Control Handlers ─────────────────────────────────────────────

  const handleSearchChange = (val: string) => {
    setQ(val);
    setPageNumber(1);
    setSuccessMsg(null);
  };

  const handleFiltroChange = (nuevoFiltro: TecnicoEstadoFiltro) => {
    setFiltro(nuevoFiltro);
    setPageNumber(1);
    setSuccessMsg(null);
  };

  const handlePageSizeChange = (nuevoSize: number) => {
    setPageSize(nuevoSize);
    setPageNumber(1);
    setSuccessMsg(null);
  };

  const handlePrevPage = () => {
    if (hasPreviousPage && pageNumber > 1) {
      setPageNumber((p) => p - 1);
      setSuccessMsg(null);
    }
  };

  const handleNextPage = () => {
    if (hasNextPage && pageNumber < totalPages) {
      setPageNumber((p) => p + 1);
      setSuccessMsg(null);
    }
  };

  const handleCloseFormModal = () => {
    detailRequestIdRef.current += 1;
    setIsFormOpen(false);
    setLoadingDetail(false);
  };

  const handleOpenCreateModal = () => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode('create');
    setSelectedTecnicoDetail(null);
    setIsFormOpen(true);
  };

  const handleOpenEditModal = async (tecnico: TecnicoListItem) => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode('edit');
    setSelectedTecnicoDetail(null);
    setIsFormOpen(true);
    setLoadingDetail(true);

    const requestId = ++detailRequestIdRef.current;

    try {
      const detail = await getTecnicoById(tecnico.id);
      if (requestId !== detailRequestIdRef.current) return;
      setSelectedTecnicoDetail(detail);
    } catch (detailError) {
      if (requestId !== detailRequestIdRef.current) return;
      setIsFormOpen(false);

      if (detailError instanceof HttpError) {
        if (detailError.status === 404) {
          await fetchTecnicosList({ q, estado: filtro, pageNumber, pageSize });
          setOpError(`El técnico "${tecnico.nombre || `#${tecnico.id}`}" ya no existe.`);
        } else if (detailError.status === 403) {
          setOpError('Acceso Denegado: No posees permisos para obtener el detalle del técnico.');
        } else if (detailError.status === 401) {
          setOpError(detailError.message || 'Sesión no autorizada o expirada.');
        } else {
          setOpError(detailError.message || 'Error al obtener la información del técnico.');
        }
      } else if (detailError instanceof Error) {
        setOpError(detailError.message);
      } else {
        setOpError('Error de conexión al cargar la información del técnico.');
      }
    } finally {
      if (requestId === detailRequestIdRef.current) setLoadingDetail(false);
    }
  };

  const handleSaveTecnico = async (
    payload: TecnicoCreatePayload | TecnicoUpdatePayload
  ) => {
    if (formMode === 'create') {
      await createTecnico(payload as TecnicoCreatePayload);
      setIsFormOpen(false);
      setPageNumber(1);
      setSuccessMsg('Técnico creado correctamente.');
      void fetchTecnicosList({ q: '', estado: filtro, pageNumber: 1, pageSize });
      return;
    }

    if (!selectedTecnicoDetail) return;

    try {
      await updateTecnico(selectedTecnicoDetail.id, payload as TecnicoUpdatePayload);
      setIsFormOpen(false);
      void fetchTecnicosList({ q, estado: filtro, pageNumber, pageSize });
      setSuccessMsg('Técnico actualizado correctamente.');
    } catch (saveError) {
      if (saveError instanceof HttpError && saveError.status === 404) {
        await fetchTecnicosList({ q, estado: filtro, pageNumber, pageSize });
      }
      throw saveError;
    }
  };

  const handleOpenDeleteModal = (tecnico: TecnicoListItem) => {
    setOpError(null);
    setSuccessMsg(null);
    setTecnicoToDelete(tecnico);
    setDeleteError(null);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!tecnicoToDelete) return;

    setIsDeleting(true);
    setDeleteError(null);

    try {
      await deleteTecnico(tecnicoToDelete.id);
      const nombre = tecnicoToDelete.nombre || `#${tecnicoToDelete.id}`;
      setIsDeleteOpen(false);
      setTecnicoToDelete(null);

      if (tecnicos.length === 1 && pageNumber > 1) {
        setPageNumber((p) => p - 1);
      } else {
        void fetchTecnicosList({ q, estado: filtro, pageNumber, pageSize });
      }
      setSuccessMsg(`Técnico "${nombre}" eliminado correctamente.`);
    } catch (deleteFailure) {
      if (deleteFailure instanceof HttpError) {
        if (deleteFailure.status === 400) {
          setDeleteError(
            deleteFailure.message ||
              'No se puede eliminar el técnico porque está asignado a solicitudes.'
          );
        } else if (deleteFailure.status === 403) {
          setDeleteError('Acceso Denegado: No posees permisos para eliminar técnicos.');
        } else if (deleteFailure.status === 404) {
          setIsDeleteOpen(false);
          setTecnicoToDelete(null);
          setDeleteError(null);
          await fetchTecnicosList({ q, estado: filtro, pageNumber, pageSize });
          setOpError('El técnico ya no existe. Se actualizó el listado.');
        } else if (deleteFailure.status === 401) {
          setDeleteError(deleteFailure.message || 'Sesión no autorizada o expirada.');
        } else {
          setDeleteError(deleteFailure.message || 'Ocurrió un error al eliminar el técnico.');
        }
      } else if (deleteFailure instanceof Error) {
        setDeleteError(deleteFailure.message);
      } else {
        setDeleteError('Error de red al intentar eliminar el técnico.');
      }
    } finally {
      setIsDeleting(false);
    }
  };

  const filtroLabel: Record<TecnicoEstadoFiltro, string> = {
    activos: 'Activos',
    inactivos: 'Inactivos',
    todos: 'Todos'
  };

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div className="mutation-header-row">
          <h2 className="preview-title mutation-page-title" style={{ margin: 0 }}>
            <Wrench size={22} aria-hidden="true" style={{ verticalAlign: 'middle', marginRight: 8 }} />
            Personal Técnico
          </h2>
          <div className="mutation-header-actions">
            <span className="count-tag" style={{ fontSize: '14px', padding: '6px 12px' }}>
              Total: {totalCount}
            </span>
            {showCreateBtn && (
              <button
                type="button"
                className="mutation-btn mutation-btn-primary"
                onClick={handleOpenCreateModal}
              >
                <Plus size={16} aria-hidden="true" />
                <span>Nuevo Técnico</span>
              </button>
            )}
          </div>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: '16px' }}>
          <strong>Gestión de Técnicos:</strong> Consulta y administración del personal disponible para solicitudes de instalación.
        </p>
      </div>

      {/* Control Bar: Búsqueda por texto y filtro por estado */}
      <div className="mutation-filter-bar" role="group" aria-label="Controles de búsqueda y filtrado">
        <div className="mutation-search-box">
          <Search size={15} aria-hidden="true" className="mutation-search-icon" />
          <input
            type="text"
            className="mutation-search-input"
            placeholder="Buscar por nombre, especialidad, correo..."
            value={q}
            onChange={(e) => handleSearchChange(e.target.value)}
          />
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Filter size={15} aria-hidden="true" className="mutation-filter-icon" />
          <span className="mutation-filter-label">Estado:</span>
          {(['activos', 'inactivos', 'todos'] as TecnicoEstadoFiltro[]).map((f) => (
            <button
              key={f}
              type="button"
              className={`mutation-filter-btn ${filtro === f ? 'mutation-filter-btn--active' : ''}`}
              onClick={() => handleFiltroChange(f)}
              aria-pressed={filtro === f}
              disabled={loading}
            >
              {filtroLabel[f]}
            </button>
          ))}
        </div>
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

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de técnicos...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">
            <AlertTriangle size={16} aria-hidden="true" style={{ verticalAlign: 'middle', marginRight: 6 }} />
            Error de Carga
          </p>
          <p className="error-desc">{error}</p>
          <button
            type="button"
            className="mutation-btn mutation-btn-secondary mutation-btn-sm mutation-retry-margin"
            onClick={() => void fetchTecnicosList({ q, estado: filtro, pageNumber, pageSize })}
          >
            <RefreshCw size={14} aria-hidden="true" />
            <span>Reintentar</span>
          </button>
        </div>
      )}

      {!loading && !error && (
        <>
          {tecnicos.length === 0 ? (
            <p className="empty-message">
              No se encontraron técnicos
              {q.trim() ? ` que coincidan con "${q}"` : ''}
              {filtro !== 'todos' ? ` con estado "${filtroLabel[filtro]}"` : ''}.
            </p>
          ) : (
            <>
              <div className="table-responsive">
                <table className="entity-table">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Nombre Completo</th>
                      <th>Especialidad</th>
                      <th>Teléfono</th>
                      <th>Correo Electrónico</th>
                      {showActionsCol && <th>Acciones</th>}
                    </tr>
                  </thead>
                  <tbody>
                    {tecnicos.map((tecnico) => (
                      <tr key={tecnico.id}>
                        <td className="text-bold">#{tecnico.id}</td>
                        <td>{tecnico.nombre || 'N/A'}</td>
                        <td>
                          <span className="specialty-badge">{tecnico.especialidad || 'General'}</span>
                        </td>
                        <td>{tecnico.telefono || 'N/A'}</td>
                        <td>{tecnico.correo || 'N/A'}</td>
                        {showActionsCol && (
                          <td>
                            <div className="mutation-action-buttons">
                              {showEditBtn && (
                                <button
                                  type="button"
                                  className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                                  onClick={() => void handleOpenEditModal(tecnico)}
                                  title="Editar técnico"
                                >
                                  <Pencil size={13} aria-hidden="true" />
                                  <span>Editar</span>
                                </button>
                              )}
                              {showDeleteBtn && (
                                <button
                                  type="button"
                                  className="mutation-btn mutation-btn-danger mutation-btn-sm"
                                  onClick={() => handleOpenDeleteModal(tecnico)}
                                  title="Eliminar técnico"
                                >
                                  <Trash2 size={13} aria-hidden="true" />
                                  <span>Eliminar</span>
                                </button>
                              )}
                            </div>
                          </td>
                        )}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {/* Barra de Paginación */}
              <div className="mutation-pagination-bar">
                <div className="mutation-pagination-info">
                  <span>Total encontrados: <strong>{totalCount}</strong></span>
                  <span>•</span>
                  <span>Página <strong>{pageNumber}</strong> de <strong>{totalPages || 1}</strong></span>
                </div>

                <div className="mutation-pagination-actions">
                  <label htmlFor="pageSizeSelectTecnicos" className="mutation-filter-label" style={{ marginRight: 4 }}>
                    Mostrar:
                  </label>
                  <select
                    id="pageSizeSelectTecnicos"
                    className="mutation-select-sm"
                    value={pageSize}
                    onChange={(e) => handlePageSizeChange(Number(e.target.value))}
                    disabled={loading}
                  >
                    <option value={10}>10</option>
                    <option value={20}>20</option>
                    <option value={50}>50</option>
                  </select>

                  <button
                    type="button"
                    className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                    onClick={handlePrevPage}
                    disabled={!hasPreviousPage || pageNumber <= 1 || loading}
                    title="Página anterior"
                  >
                    <ChevronLeft size={14} aria-hidden="true" />
                    <span>Anterior</span>
                  </button>

                  <button
                    type="button"
                    className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                    onClick={handleNextPage}
                    disabled={!hasNextPage || pageNumber >= totalPages || loading}
                    title="Página siguiente"
                  >
                    <span>Siguiente</span>
                    <ChevronRight size={14} aria-hidden="true" />
                  </button>
                </div>
              </div>
            </>
          )}
        </>
      )}

      <div className="note" style={{ marginTop: '24px' }}>
        {showActionsCol ? (
          <>
            <strong>Administración:</strong> Puede registrar y actualizar técnicos. La eliminación se bloqueará cuando existan solicitudes asociadas.
          </>
        ) : (
          <>
            <strong>Modo de Consulta:</strong> El rol Técnico puede consultar el catálogo, sin permisos de creación, edición o eliminación.
          </>
        )}
      </div>

      <TecnicoFormModal
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedTecnicoDetail}
        isLoadingDetail={loadingDetail}
        onSave={handleSaveTecnico}
        onClose={handleCloseFormModal}
      />

      <ConfirmModal
        isOpen={isDeleteOpen}
        title="Confirmar Eliminación"
        message={
          tecnicoToDelete
            ? `¿Está seguro que desea eliminar al técnico "${tecnicoToDelete.nombre}"? Esta acción no se puede deshacer.`
            : '¿Está seguro que desea eliminar este técnico?'
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
            setTecnicoToDelete(null);
            setDeleteError(null);
          }
        }}
      />
    </div>
  );
}

