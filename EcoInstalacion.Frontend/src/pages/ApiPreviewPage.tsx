import { useEffect, useState } from 'react';
import { getClientes } from '../features/clientes/clientesApi';
import { getTecnicos } from '../features/tecnicos/tecnicosApi';
import type { ClienteListItem } from '../features/clientes/types';
import type { TecnicoListItem } from '../features/tecnicos/types';
import { API_BASE_URL } from '../config/environment';

export default function ApiPreviewPage() {
  const [clientes, setClientes] = useState<ClienteListItem[]>([]);
  const [tecnicos, setTecnicos] = useState<TecnicoListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadData() {
      try {
        setLoading(true);
        setError(null);
        
        const [clientesData, tecnicosData] = await Promise.all([
          getClientes(),
          getTecnicos()
        ]);
        
        setClientes(clientesData);
        setTecnicos(tecnicosData);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError('No se pudo conectar con el servidor backend.');
        }
      } finally {
        setLoading(false);
      }
    }

    loadData();
  }, []);

  return (
    <div className="preview-container">
      <div className="preview-header">
        <h2 className="preview-title">API Integration Preview</h2>
        <div className="connection-info">
          <span className="status-label">Backend Connection Status: </span>
          {loading ? (
            <span className="status-badge status-loading">Checking...</span>
          ) : error ? (
            <span className="status-badge status-error">Offline</span>
          ) : (
            <span className="status-badge status-success">Online</span>
          )}
        </div>
        <div className="api-info-badge">
          API Base URL: <code>{API_BASE_URL}</code>
        </div>
        <p className="preview-mode-tag">
          🔒 <strong>Read-Only Integration:</strong> Write operations, forms, and navigation are disabled in this sprint.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando datos de clientes y técnicos desde el backend .NET...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">⚠️ Error de Conexión</p>
          <p className="error-desc">{error}</p>
          <p className="error-help">Por favor, asegúrate de que el backend se está ejecutando en <code>{API_BASE_URL}</code> y que CORS está habilitado.</p>
        </div>
      )}

      {!loading && !error && (
        <div className="preview-content">
          <div className="data-section">
            <div className="section-header">
              <h3>Clientes</h3>
              <span className="count-tag">{clientes.length} registrados</span>
            </div>
            
            {clientes.length === 0 ? (
              <p className="empty-message">No hay clientes registrados en la base de datos.</p>
            ) : (
              <div className="simple-list">
                {clientes.slice(0, 5).map(c => (
                  <div key={c.id} className="list-card">
                    <p className="card-name">{c.nombreCompleto}</p>
                    <p className="card-detail">DUI: {c.dui || 'N/A'}</p>
                    <p className="card-detail">Tel: {c.telefono || 'N/A'}</p>
                    <p className="card-detail">Email: {c.correo || 'N/A'}</p>
                  </div>
                ))}
                {clientes.length > 5 && (
                  <p className="more-records-msg">... y {clientes.length - 5} clientes más.</p>
                )}
              </div>
            )}
          </div>

          <div className="data-section">
            <div className="section-header">
              <h3>Técnicos</h3>
              <span className="count-tag">{tecnicos.length} registrados</span>
            </div>
            
            {tecnicos.length === 0 ? (
              <p className="empty-message">No hay técnicos registrados en la base de datos.</p>
            ) : (
              <div className="simple-list">
                {tecnicos.slice(0, 5).map(t => (
                  <div key={t.id} className="list-card">
                    <p className="card-name">{t.nombre}</p>
                    <p className="card-detail font-medium">Especialidad: {t.especialidad || 'N/A'}</p>
                    <p className="card-detail">Tel: {t.telefono || 'N/A'}</p>
                    <p className="card-detail">Email: {t.correo || 'N/A'}</p>
                  </div>
                ))}
                {tecnicos.length > 5 && (
                  <p className="more-records-msg">... y {tecnicos.length - 5} técnicos más.</p>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
