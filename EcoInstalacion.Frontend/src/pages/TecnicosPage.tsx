import { useEffect, useState } from "react";
import { getTecnicos } from "../features/tecnicos/tecnicosApi";
import type { TecnicoListItem } from "../features/tecnicos/types";

export default function TecnicosPage() {
  const [tecnicos, setTecnicos] = useState<TecnicoListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchTecnicos() {
      try {
        setLoading(true);
        setError(null);
        const data = await getTecnicos();
        setTecnicos(data);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("Error de red al cargar el catálogo de técnicos.");
        }
      } finally {
        setLoading(false);
      }
    }
    fetchTecnicos();
  }, []);

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "10px" }}>
          <h2 className="preview-title" style={{ margin: 0 }}>Personal Técnico</h2>
          <span className="count-tag" style={{ fontSize: "14px", padding: "6px 12px" }}>
            Total: {tecnicos.length}
          </span>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: "16px" }}>
          🔧 <strong>Visualización de Técnicos:</strong> Consulta de personal técnico y especialidades certificadas para campo.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de técnicos...</p>
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
          {tecnicos.length === 0 ? (
            <p className="empty-message">No se encontraron técnicos registrados en el sistema.</p>
          ) : (
            <div className="table-responsive">
              <table className="entity-table">
                <thead>
                  <tr>
                    <th>ID</th>
                    <th>Nombre Completo</th>
                    <th>Especialidad</th>
                    <th>Teléfono</th>
                    <th>Correo Electrónico</th>
                  </tr>
                </thead>
                <tbody>
                  {tecnicos.map((t) => (
                    <tr key={t.id}>
                      <td className="text-bold">#{t.id}</td>
                      <td>{t.nombre || "N/A"}</td>
                      <td>
                        <span className="specialty-badge">{t.especialidad || "General"}</span>
                      </td>
                      <td>{t.telefono || "N/A"}</td>
                      <td>{t.correo || "N/A"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <div className="note" style={{ marginTop: "24px" }}>
        🔒 <strong>Modo de Consulta Seguro:</strong> El alta de personal técnico está restringida al panel administrativo del backend.
      </div>
    </div>
  );
}
