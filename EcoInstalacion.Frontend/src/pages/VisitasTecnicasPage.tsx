import { useEffect, useState } from "react";
import { getVisitasTecnicas } from "../features/visitasTecnicas/visitasTecnicasApi";
import type { VisitaTecnicaListItem } from "../features/visitasTecnicas/types";

export default function VisitasTecnicasPage() {
  const [visitas, setVisitas] = useState<VisitaTecnicaListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchVisitas() {
      try {
        setLoading(true);
        setError(null);
        const data = await getVisitasTecnicas();
        setVisitas(data);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("Error de red al cargar el registro de visitas técnicas.");
        }
      } finally {
        setLoading(false);
      }
    }
    fetchVisitas();
  }, []);

  const aprobadas = visitas.filter((v) => v.aprobado === true).length;
  const pendientes = visitas.filter((v) => v.aprobado !== true).length;

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "10px" }}>
          <h2 className="preview-title" style={{ margin: 0 }}>Registro de Visitas Técnicas</h2>
          <span className="count-tag" style={{ fontSize: "14px", padding: "6px 12px" }}>
            Total: {visitas.length}
          </span>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: "16px" }}>
          📐 <strong>Control de Visitas Técnicas:</strong> Historial de inspecciones técnicas en sitio y su estado de aprobación.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de visitas...</p>
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
          <div className="metrics-grid" style={{ marginBottom: "24px" }}>
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

          {visitas.length === 0 ? (
            <p className="empty-message">No se encontraron visitas técnicas registradas.</p>
          ) : (
            <div className="table-responsive">
              <table className="entity-table">
                <thead>
                  <tr>
                    <th>ID Visita</th>
                    <th>ID Solicitud</th>
                    <th>Fecha de Visita</th>
                    <th>Estado de Aprobación</th>
                    <th>Observaciones</th>
                  </tr>
                </thead>
                <tbody>
                  {visitas.map((v) => (
                    <tr key={v.id}>
                      <td className="text-bold">#{v.id}</td>
                      <td>Solicitud #{v.solicitudId}</td>
                      <td>{v.fechaVisita ? new Date(v.fechaVisita).toLocaleDateString() : "N/A"}</td>
                      <td>
                        {v.aprobado === true ? (
                          <span className="status-pill badge-success">Aprobada</span>
                        ) : (
                          <span className="status-pill badge-warning">Pendiente / No Aprobada</span>
                        )}
                      </td>
                      <td>
                        <span className="visit-observations">
                          {v.observaciones || <span className="text-muted">Sin observaciones registradas</span>}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <div className="note" style={{ marginTop: "24px" }}>
        🔒 <strong>Modo de Consulta Seguro:</strong> El ingreso de informes y cambio de estado de aprobación se realiza en la app de escritorio/MVC.
      </div>
    </div>
  );
}
