import { useEffect, useState } from "react";
import { getSolicitudes } from "../features/solicitudes/solicitudesApi";
import type { SolicitudListItem } from "../features/solicitudes/types";

export default function SolicitudesPage() {
  const [solicitudes, setSolicitudes] = useState<SolicitudListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchSolicitudes() {
      try {
        setLoading(true);
        setError(null);
        const data = await getSolicitudes();
        setSolicitudes(data);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("Error de red al cargar las solicitudes de instalación.");
        }
      } finally {
        setLoading(false);
      }
    }
    fetchSolicitudes();
  }, []);

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "10px" }}>
          <h2 className="preview-title" style={{ margin: 0 }}>Solicitudes de Instalación</h2>
          <span className="count-tag" style={{ fontSize: "14px", padding: "6px 12px" }}>
            Total: {solicitudes.length}
          </span>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: "16px" }}>
          📋 <strong>Control de Solicitudes:</strong> Monitoreo de solicitudes de servicios residenciales y solares activos.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de solicitudes...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">⚠️ Error de Carga</p>
          <p className="error-desc">{error}</p>
        </div>
      )}

      {!loading && !error && (
        <>
          {solicitudes.length === 0 ? (
            <p className="empty-message">No se encontraron solicitudes registradas en el sistema.</p>
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
                  </tr>
                </thead>
                <tbody>
                  {solicitudes.map((s) => {
                    let statusClass = "badge-secondary";
                    const sEstado = s.estado?.toLowerCase();
                    if (sEstado === "pendiente" || sEstado === "creado") statusClass = "badge-warning";
                    if (sEstado === "aprobado" || sEstado === "finalizado" || sEstado === "completado") statusClass = "badge-success";
                    if (sEstado === "cancelado" || sEstado === "rechazado") statusClass = "badge-danger";

                    return (
                      <tr key={s.id}>
                        <td className="text-bold">#{s.id}</td>
                        <td>{s.fechaSolicitud ? new Date(s.fechaSolicitud).toLocaleDateString() : "N/A"}</td>
                        <td>{s.clienteNombre || `Cliente ID: ${s.clienteId || "N/A"}`}</td>
                        <td>
                          <span className="service-type-tag">{s.tipoInstalacion || "Instalación Básica"}</span>
                        </td>
                        <td>{s.tecnicoNombre || <span className="text-muted">No asignado</span>}</td>
                        <td>
                          <span className={`status-pill ${statusClass}`}>{s.estado || "Creado"}</span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <div className="note" style={{ marginTop: "24px" }}>
        🔒 <strong>Modo de Consulta Seguro:</strong> La creación de nuevas solicitudes de instalación se gestiona mediante la aplicación MVC/Razor del backend.
      </div>
    </div>
  );
}
