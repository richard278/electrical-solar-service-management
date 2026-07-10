import { useEffect, useState } from 'react';
import { getSolicitudes } from '../features/solicitudes/solicitudesApi';
import { getVisitasTecnicas } from '../features/visitasTecnicas/visitasTecnicasApi';
import type { SolicitudListItem } from '../features/solicitudes/types';
import type { VisitaTecnicaListItem } from '../features/visitasTecnicas/types';

export default function OperationalDashboardPage() {
  const [solicitudes, setSolicitudes] = useState<SolicitudListItem[]>([]);
  const [visitas, setVisitas] = useState<VisitaTecnicaListItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadOperationalData() {
      try {
        setLoading(true);
        setError(null);

        const [solicitudesData, visitasData] = await Promise.all([
          getSolicitudes(),
          getVisitasTecnicas()
        ]);

        setSolicitudes(solicitudesData);
        setVisitas(visitasData);
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

    loadOperationalData();
  }, []);

  const visitasAprobadas = visitas.filter(v => v.aprobado === true).length;
  const visitasPendientes = visitas.filter(v => v.aprobado !== true).length;

  return (
    <div className="preview-container operational-dashboard">
      <div className="preview-header">
        <h2 className="preview-title">Operational Dashboard</h2>
        <p className="preview-mode-tag text-teal">
          📋 <strong>Read-Only Operational View:</strong> Visualización operativa en tiempo real de las solicitudes de instalación eléctrica/solar y visitas técnicas de campo.
        </p>
      </div>

      {loading && (
        <div className="loading-placeholder">
          <div className="spinner"></div>
          <p>Cargando información operativa desde el backend .NET...</p>
        </div>
      )}

      {error && (
        <div className="error-alert">
          <p className="error-title">⚠️ Error de Carga Operativa</p>
          <p className="error-desc">{error}</p>
        </div>
      )}

      {!loading && !error && (
        <>
          <div className="metrics-grid">
            <div className="metric-card">
              <span className="metric-label">Total Solicitudes</span>
              <span className="metric-value">{solicitudes.length}</span>
            </div>
            <div className="metric-card">
              <span className="metric-label">Total Visitas</span>
              <span className="metric-value">{visitas.length}</span>
            </div>
            <div className="metric-card border-green">
              <span className="metric-label">Visitas Aprobadas</span>
              <span className="metric-value text-green">{visitasAprobadas}</span>
            </div>
            <div className="metric-card border-amber">
              <span className="metric-label">Visitas Pendientes</span>
              <span className="metric-value text-amber">{visitasPendientes}</span>
            </div>
          </div>

          <div className="preview-content">
            <div className="data-section">
              <div className="section-header">
                <h3>Solicitudes de Instalación</h3>
                <span className="count-tag">{solicitudes.length} en total</span>
              </div>

              {solicitudes.length === 0 ? (
                <p className="empty-message">No hay solicitudes de instalación registradas.</p>
              ) : (
                <div className="simple-list">
                  {solicitudes.slice(0, 5).map(s => {
                    let statusClass = 'badge-secondary';
                    const sEstado = s.estado?.toLowerCase();
                    if (sEstado === 'pendiente' || sEstado === 'creado') statusClass = 'badge-warning';
                    if (sEstado === 'aprobado' || sEstado === 'finalizado' || sEstado === 'completado') statusClass = 'badge-success';

                    return (
                      <div key={s.id} className="list-card">
                        <div className="list-card-header">
                          <span className="card-name">Solicitud #{s.id}</span>
                          <span className={`status-pill ${statusClass}`}>{s.estado || 'Sin estado'}</span>
                        </div>
                        <p className="card-detail font-medium">{s.tipoInstalacion || 'Instalación básica'}</p>
                        <p className="card-detail">Cliente: {s.clienteNombre || 'N/A'}</p>
                        <p className="card-detail">Técnico: {s.tecnicoNombre || 'No asignado'}</p>
                      </div>
                    );
                  })}
                  {solicitudes.length > 5 && (
                    <p className="more-records-msg">... y {solicitudes.length - 5} solicitudes más.</p>
                  )}
                </div>
              )}
            </div>

            <div className="data-section">
              <div className="section-header">
                <h3>Visitas Técnicas</h3>
                <span className="count-tag">{visitas.length} en total</span>
              </div>

              {visitas.length === 0 ? (
                <p className="empty-message">No hay visitas técnicas registradas.</p>
              ) : (
                <div className="simple-list">
                  {visitas.slice(0, 5).map(v => (
                    <div key={v.id} className="list-card">
                      <div className="list-card-header">
                        <span className="card-name">Visita #{v.id}</span>
                        {v.aprobado === true ? (
                          <span className="status-pill badge-success">Aprobada</span>
                        ) : (
                          <span className="status-pill badge-warning">Pendiente / No Aprobada</span>
                        )}
                      </div>
                      <p className="card-detail font-medium">Asociada a Solicitud #{v.solicitudId}</p>
                      <p className="card-detail">Fecha: {v.fechaVisita ? new Date(v.fechaVisita).toLocaleDateString() : 'N/A'}</p>
                      {v.observaciones && <p className="card-desc">Observaciones: "{v.observaciones}"</p>}
                    </div>
                  ))}
                  {visitas.length > 5 && (
                    <p className="more-records-msg">... y {visitas.length - 5} visitas más.</p>
                  )}
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
