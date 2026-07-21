import { useEffect, useState } from "react";
import { getClientes } from "../features/clientes/clientesApi";
import { getTecnicos } from "../features/tecnicos/tecnicosApi";
import { getSolicitudes } from "../features/solicitudes/solicitudesApi";
import { getVisitasTecnicas } from "../features/visitasTecnicas/visitasTecnicasApi";
import { API_BASE_URL } from "../config/environment";

export default function OverviewPage() {
  const [metrics, setMetrics] = useState({
    clientes: 0,
    tecnicos: 0,
    solicitudes: 0,
    visitas: 0,
    visitasAprobadas: 0,
    visitasPendientes: 0,
  });
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchMetrics() {
      try {
        setLoading(true);
        setError(null);
        const [clientesData, tecnicosData, solicitudesData, visitasData] =
          await Promise.all([
            getClientes(),
            getTecnicos(),
            getSolicitudes(),
            getVisitasTecnicas(),
          ]);

        const aprobadas = visitasData.filter((v) => v.aprobado === true).length;
        const pendientes = visitasData.filter((v) => v.aprobado !== true).length;

        setMetrics({
          clientes: clientesData.length,
          tecnicos: tecnicosData.length,
          solicitudes: solicitudesData.length,
          visitas: visitasData.length,
          visitasAprobadas: aprobadas,
          visitasPendientes: pendientes,
        });
      } catch (err: any) {
        if (err && err.status === 401) {
          setError("Sesión no autorizada o expirada.");
        } else if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("Error de conexión al obtener las métricas.");
        }
      } finally {
        setLoading(false);
      }
    }

    fetchMetrics();
  }, []);

  return (
    <div className="overview-page">
      <div className="preview-header">
        <h2 className="preview-title">Resumen General del Sistema</h2>
        <p className="preview-mode-tag text-teal">
          🏠 <strong>Panel de Control Operativo:</strong> Vista general consolidada del estado y volumen de operaciones de EcoInstalacion.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando resumen de datos del backend .NET...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">
            {error.includes("Sesión") || error.includes("autorizada") ? "⚠️ Acceso Denegado" : "⚠️ Error de Conexión"}
          </p>
          <p className="error-desc">{error}</p>
          {!error.includes("Sesión") && !error.includes("autorizada") && (
            <p className="error-help">Por favor, asegúrate de que el backend se está ejecutando y que CORS está configurado.</p>
          )}
        </div>
      )}

      {!loading && !error && (
        <>
          <div className="metrics-grid">
            <div className="metric-card">
              <span className="metric-label">Clientes</span>
              <span className="metric-value">{metrics.clientes}</span>
            </div>
            <div className="metric-card">
              <span className="metric-label">Técnicos</span>
              <span className="metric-value">{metrics.tecnicos}</span>
            </div>
            <div className="metric-card">
              <span className="metric-label">Solicitudes</span>
              <span className="metric-value">{metrics.solicitudes}</span>
            </div>
            <div className="metric-card border-green">
              <span className="metric-label">Visitas Aprobadas</span>
              <span className="metric-value text-green">{metrics.visitasAprobadas}</span>
            </div>
          </div>

          <div className="overview-details">
            <div className="detail-section">
              <h3>Estado de Integración API</h3>
              <div className="api-info-badge">
                URL Base de la API: <code>{API_BASE_URL}</code>
              </div>
              <div className="connection-info" style={{ marginTop: "12px" }}>
                <span className="status-label">Conexión con Servidor: </span>
                <span className="status-badge status-success">Online</span>
              </div>
            </div>

            <div className="detail-section milestone-card">
              <h3>Seguridad de la Aplicación</h3>
              <div className="status-pill badge-warning" style={{ display: "inline-block", marginBottom: "12px" }}>
                Activa
              </div>
              <p style={{ margin: "0 0 10px 0", fontSize: "14px", fontWeight: "600" }}>
                Autenticación JWT / Acceso por roles
              </p>
              <p style={{ margin: 0, fontSize: "13px", color: "var(--text-light)" }} className="milestone-desc">
                La interfaz consume rutas protegidas mediante JSON Web Tokens (JWT) y presenta las operaciones disponibles de acuerdo con el rol autenticado.
              </p>
            </div>
          </div>
        </>
      )}

      <div className="note" style={{ marginTop: "32px" }}>
        ℹ️ <strong>Nota de Seguridad:</strong> El acceso al panel y a las operaciones de gestión está protegido mediante autenticación JWT y autorización por roles.
      </div>
    </div>
  );
}
