import { useEffect, useState } from "react";
import { getClientes } from "../features/clientes/clientesApi";
import type { ClienteListItem } from "../features/clientes/types";

export default function ClientesPage() {
  const [clientes, setClientes] = useState<ClienteListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchClientes() {
      try {
        setLoading(true);
        setError(null);
        const data = await getClientes();
        setClientes(data);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("Error de red al cargar el catálogo de clientes.");
        }
      } finally {
        setLoading(false);
      }
    }
    fetchClientes();
  }, []);

  return (
    <div className="entity-panel">
      <div className="preview-header">
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "10px" }}>
          <h2 className="preview-title" style={{ margin: 0 }}>Catálogo de Clientes</h2>
          <span className="count-tag" style={{ fontSize: "14px", padding: "6px 12px" }}>
            Total: {clientes.length}
          </span>
        </div>
        <p className="preview-mode-tag text-teal" style={{ marginTop: "16px" }}>
          👥 <strong>Visualización de Clientes:</strong> Consulta detallada de información de contacto y domicilio de clientes registrados.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando lista de clientes...</p>
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
          {clientes.length === 0 ? (
            <p className="empty-message">No se encontraron clientes en el sistema.</p>
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
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      <div className="note" style={{ marginTop: "24px" }}>
        🔒 <strong>Modo de Consulta Seguro:</strong> Registro, edición y eliminación de clientes inhabilitados en este sprint.
      </div>
    </div>
  );
}
