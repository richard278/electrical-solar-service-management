import { useEffect, useState, useCallback, useRef } from "react";
import {
  Plus,
  Pencil,
  Trash2,
  RefreshCw,
  AlertTriangle,
  CheckCircle,
  Users,
  Filter
} from "lucide-react";
import { useAuth } from "../auth/useAuth";
import {
  canCreateCliente,
  canEditCliente,
  canDeleteCliente
} from "../utils/roleUtils";
import {
  getClientes,
  getClientesAdministracion,
  getClienteById,
  createCliente,
  updateCliente,
  deleteCliente
} from "../features/clientes/clientesApi";
import type {
  ClienteListItem,
  ClienteDetail,
  ClienteCreatePayload,
  ClienteUpdatePayload,
  ClienteEstadoFiltro
} from "../features/clientes/types";
import { ClienteFormModal } from "../features/clientes/components/ClienteFormModal";
import { ConfirmModal } from "../components/common/ConfirmModal";
import { HttpError } from "../services/httpClient";
import { normalizeRole } from "../utils/roleUtils";
import "../components/common/MutationUi.css";

export default function ClientesPage() {
  const { user } = useAuth();
  const isAdmin = normalizeRole(user?.rol) === "administrador";

  // Catalog list state
  const [clientes, setClientes] = useState<ClienteListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const requestIdRef = useRef<number>(0);
  const detailRequestIdRef = useRef<number>(0);

  // Success / operational messages
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [opError, setOpError] = useState<string | null>(null);

  // Filter state — only meaningful for Admin; Técnico always uses "activos"
  const [filtro, setFiltro] = useState<ClienteEstadoFiltro>("activos");

  // Form Modal state
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [formMode, setFormMode] = useState<"create" | "edit">("create");
  const [selectedClienteDetail, setSelectedClienteDetail] = useState<ClienteDetail | null>(null);
  const [loadingDetail, setLoadingDetail] = useState<boolean>(false);

  // Delete Modal state
  const [isDeleteOpen, setIsDeleteOpen] = useState<boolean>(false);
  const [clienteToDelete, setClienteToDelete] = useState<ClienteListItem | null>(null);
  const [isDeleting, setIsDeleting] = useState<boolean>(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const showCreateBtn = canCreateCliente(user);
  const showEditBtn = canEditCliente(user);
  const showDeleteBtn = canDeleteCliente(user);
  const showActionsCol = showEditBtn || showDeleteBtn;

  // ─── Fetch ────────────────────────────────────────────────────────

  const fetchClientesList = useCallback(
    async (requestedFiltro: ClienteEstadoFiltro) => {
      const requestId = ++requestIdRef.current;
      setLoading(true);
      setError(null);
      setOpError(null);
      try {
        const data = isAdmin
          ? await getClientesAdministracion(requestedFiltro)
          : await getClientes();

        if (requestId !== requestIdRef.current) return;
        setClientes(data);
      } catch (err) {
        if (requestId !== requestIdRef.current) return;
        if (err instanceof HttpError) {
          if (err.status === 403) {
            setError("Acceso Denegado: Tu rol no tiene permisos para consultar el catálogo de clientes.");
          } else if (err.status === 401) {
            setError(err.message || "Sesión no autorizada o expirada.");
          } else {
            setError(err.message || "Error al cargar la lista de clientes.");
          }
        } else if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("Error de red al cargar el catálogo de clientes.");
        }
      } finally {
        if (requestId === requestIdRef.current) {
          setLoading(false);
        }
      }
    },
    [isAdmin]
  );

  useEffect(() => {
    void fetchClientesList(filtro);
  }, [fetchClientesList, filtro]);

  const handleFiltroChange = (nuevoFiltro: ClienteEstadoFiltro) => {
    setFiltro(nuevoFiltro);
    setSuccessMsg(null);
  };

  const handleCloseFormModal = () => {
    detailRequestIdRef.current += 1;
    setIsFormOpen(false);
    setLoadingDetail(false);
  };

  // ─── Form Modal Handlers ──────────────────────────────────────────

  const handleOpenCreateModal = () => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode("create");
    setSelectedClienteDetail(null);
    setIsFormOpen(true);
  };

  const handleOpenEditModal = async (cliente: ClienteListItem) => {
    setOpError(null);
    setSuccessMsg(null);
    setFormMode("edit");
    setSelectedClienteDetail(null);
    setIsFormOpen(true);
    setLoadingDetail(true);

    const requestId = ++detailRequestIdRef.current;

    try {
      const detail = await getClienteById(cliente.id);
      if (requestId !== detailRequestIdRef.current) return;
      setSelectedClienteDetail(detail);
    } catch (err) {
      if (requestId !== detailRequestIdRef.current) return;
      setIsFormOpen(false);
      if (err instanceof HttpError) {
        if (err.status === 404) {
          await fetchClientesList(filtro);
          setOpError(
            `El cliente "${cliente.nombreCompleto || `#${cliente.id}`}" ya no existe en el servidor.`
          );
        } else if (err.status === 403) {
          setOpError("Acceso Denegado: No posees permisos para obtener el detalle de este cliente.");
        } else if (err.status === 401) {
          setOpError(err.message || "Sesión no autorizada o expirada.");
        } else {
          setOpError(err.message || "Error al obtener la información del cliente.");
        }
      } else if (err instanceof Error) {
        setOpError(err.message);
      } else {
        setOpError("Error de conexión al cargar la información del cliente.");
      }
    } finally {
      if (requestId === detailRequestIdRef.current) {
        setLoadingDetail(false);
      }
    }
  };

  const handleSaveCliente = async (
    payload: ClienteCreatePayload | ClienteUpdatePayload
  ) => {
    if (formMode === "create") {
      await createCliente(payload as ClienteCreatePayload);
      setIsFormOpen(false);
      // Clientes nuevos son activos — si el filtro es otro, cambiar a "activos"
      if (isAdmin && filtro !== "activos") {
        setFiltro("activos");
      } else {
        await fetchClientesList(filtro);
      }
      setSuccessMsg("Cliente creado correctamente. Visualizando clientes activos.");
    } else if (selectedClienteDetail) {
      const updPayload = payload as ClienteUpdatePayload;
      try {
        await updateCliente(selectedClienteDetail.id, updPayload);
        setIsFormOpen(false);
        await fetchClientesList(filtro);
        const statusMsg = updPayload.esActivo
          ? "Cliente actualizado y marcado como Activo."
          : "Cliente actualizado y marcado como Inactivo.";
        setSuccessMsg(statusMsg);
      } catch (err) {
        if (err instanceof HttpError && err.status === 404) {
          await fetchClientesList(filtro);
        }
        throw err; // Let ClienteFormModal display the error
      }
    }
  };

  // ─── Delete Modal Handlers ────────────────────────────────────────

  const handleOpenDeleteModal = (cliente: ClienteListItem) => {
    setOpError(null);
    setSuccessMsg(null);
    setClienteToDelete(cliente);
    setDeleteError(null);
    setIsDeleteOpen(true);
  };

  const handleConfirmDelete = async () => {
    if (!clienteToDelete) return;
    setIsDeleting(true);
    setDeleteError(null);

    try {
      await deleteCliente(clienteToDelete.id);
      const nombre = clienteToDelete.nombreCompleto || `#${clienteToDelete.id}`;
      setIsDeleteOpen(false);
      setClienteToDelete(null);
      await fetchClientesList(filtro);
      setSuccessMsg(`Cliente "${nombre}" eliminado correctamente.`);
    } catch (err) {
      if (err instanceof HttpError) {
        if (err.status === 400) {
          setDeleteError(
            err.message ||
              "No se puede eliminar el cliente porque tiene registros o solicitudes asociadas."
          );
        } else if (err.status === 403) {
          setDeleteError("Acceso Denegado: No posees permisos para eliminar este cliente.");
        } else if (err.status === 404) {
          setIsDeleteOpen(false);
          setClienteToDelete(null);
          setDeleteError(null);
          await fetchClientesList(filtro);
          setOpError("El cliente ya no existe o fue eliminado previamente. Se actualizó el listado.");
        } else if (err.status === 401) {
          setDeleteError(err.message || "Sesión no autorizada o expirada.");
        } else {
          setDeleteError(
            err.message || "Ocurrió un error al intentar eliminar el cliente."
          );
        }
      } else if (err instanceof Error) {
        setDeleteError(err.message);
      } else {
        setDeleteError("Error de red al intentar eliminar el cliente.");
      }
    } finally {
      setIsDeleting(false);
    }
  };

  // ─── Helpers ─────────────────────────────────────────────────────

  const filtroLabel: Record<ClienteEstadoFiltro, string> = {
    activos: "Activos",
    inactivos: "Inactivos",
    todos: "Todos",
  };

  // ─── Render ───────────────────────────────────────────────────────

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div className="mutation-header-row">
          <h2 className="preview-title mutation-page-title" style={{ margin: 0 }}>
            <Users size={22} aria-hidden="true" style={{ verticalAlign: "middle", marginRight: 8 }} />
            Catálogo de Clientes
          </h2>
          <div className="mutation-header-actions">
            <span className="count-tag" style={{ fontSize: "14px", padding: "6px 12px" }}>
              Total: {clientes.length}
            </span>
            {showCreateBtn && (
              <button
                type="button"
                className="mutation-btn mutation-btn-primary"
                onClick={handleOpenCreateModal}
              >
                <Plus size={16} aria-hidden="true" />
                <span>Nuevo Cliente</span>
              </button>
            )}
          </div>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: "16px" }}>
          <strong>Gestión de Clientes:</strong> Consulta, registro y administración de información de contacto de clientes.
        </p>
      </div>

      {/* Filtro por estado — solo Administrador */}
      {isAdmin && (
        <div className="mutation-filter-bar" role="group" aria-label="Filtrar clientes por estado">
          <Filter size={15} aria-hidden="true" className="mutation-filter-icon" />
          <span className="mutation-filter-label">Estado:</span>
          {(["activos", "inactivos", "todos"] as ClienteEstadoFiltro[]).map((f) => (
            <button
              key={f}
              type="button"
              className={`mutation-filter-btn ${filtro === f ? "mutation-filter-btn--active" : ""}`}
              onClick={() => handleFiltroChange(f)}
              aria-pressed={filtro === f}
              disabled={loading}
            >
              {filtroLabel[f]}
            </button>
          ))}
        </div>
      )}

      {/* Success banner */}
      {successMsg && (
        <div
          className="mutation-success-banner"
          role="status"
          aria-live="polite"
        >
          <CheckCircle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
          <span>{successMsg}</span>
        </div>
      )}

      {/* Page-level operational error banner */}
      {opError && (
        <div
          className="mutation-page-op-error"
          role="alert"
          aria-live="polite"
        >
          <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
          <span>{opError}</span>
        </div>
      )}

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de clientes...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">
            <AlertTriangle size={16} aria-hidden="true" style={{ verticalAlign: "middle", marginRight: 6 }} />
            Error de Carga
          </p>
          <p className="error-desc">{error}</p>
          <button
            type="button"
            className="mutation-btn mutation-btn-secondary mutation-btn-sm mutation-retry-margin"
            onClick={() => fetchClientesList(filtro)}
          >
            <RefreshCw size={14} aria-hidden="true" />
            <span>Reintentar</span>
          </button>
        </div>
      )}

      {!loading && !error && (
        <>
          {clientes.length === 0 ? (
            <p className="empty-message">
              No se encontraron clientes
              {isAdmin && filtro !== "todos" ? ` con estado "${filtroLabel[filtro]}"` : ""}.
            </p>
          ) : (
            <div className="table-responsive">
              <table className="entity-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Nombre Completo</th>
                    <th>DUI</th>
                    <th>Teléfono</th>
                    <th>Correo</th>
                    <th>Dirección</th>
                    <th>Estado</th>
                    {showActionsCol && <th>Acciones</th>}
                  </tr>
                </thead>
                <tbody>
                  {clientes.map((c) => (
                    <tr key={c.id}>
                      <td className="text-bold">#{c.id}</td>
                      <td>{c.nombreCompleto || "N/A"}</td>
                      <td>{c.dui || "N/A"}</td>
                      <td>{c.telefono || "N/A"}</td>
                      <td>{c.correo || "N/A"}</td>
                      <td>{c.direccion || "N/A"}</td>
                      <td>
                        {c.esActivo !== false ? (
                          <span className="status-pill badge-success">Activo</span>
                        ) : (
                          <span className="status-pill badge-secondary">Inactivo</span>
                        )}
                      </td>
                      {showActionsCol && (
                        <td>
                          <div className="mutation-action-buttons">
                            {showEditBtn && (
                              <button
                                type="button"
                                className="mutation-btn mutation-btn-secondary mutation-btn-sm"
                                onClick={() => handleOpenEditModal(c)}
                                title="Editar cliente"
                              >
                                <Pencil size={13} aria-hidden="true" />
                                <span>Editar</span>
                              </button>
                            )}
                            {showDeleteBtn && (
                              <button
                                type="button"
                                className="mutation-btn mutation-btn-danger mutation-btn-sm"
                                onClick={() => handleOpenDeleteModal(c)}
                                title="Eliminar cliente"
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
          )}
        </>
      )}

      {/* Modal Formulario Cliente */}
      <ClienteFormModal
        isOpen={isFormOpen}
        mode={formMode}
        initialData={selectedClienteDetail}
        isLoadingDetail={loadingDetail}
        isAdmin={isAdmin}
        onSave={handleSaveCliente}
        onClose={handleCloseFormModal}
      />

      {/* Modal Confirmación Eliminación */}
      <ConfirmModal
        isOpen={isDeleteOpen}
        title="Confirmar Eliminación"
        message={
          clienteToDelete
            ? `¿Está seguro que desea eliminar al cliente "${clienteToDelete.nombreCompleto}" (DUI: ${
                clienteToDelete.dui || "N/A"
              })? Esta acción no se puede deshacer.`
            : "¿Está seguro que desea eliminar este cliente?"
        }
        confirmText="Sí, Eliminar"
        cancelText="Cancelar"
        confirmIcon={<Trash2 size={14} />}
        isSubmitting={isDeleting}
        error={deleteError}
        onConfirm={handleConfirmDelete}
        onCancel={() => {
          if (!isDeleting) {
            setIsDeleteOpen(false);
            setClienteToDelete(null);
            setDeleteError(null);
          }
        }}
      />
    </div>
  );
}
