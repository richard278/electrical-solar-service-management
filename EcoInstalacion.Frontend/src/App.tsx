import ApiPreviewPage from './pages/ApiPreviewPage'
import OperationalDashboardPage from './pages/OperationalDashboardPage'
import './App.css'

function App() {
  return (
    <div className="container">
      <div className="card">
        <div className="card-header">
          <div className="badge">API integration Sprint 5D</div>
        </div>
        <div className="card-body">
          <h1 className="title">EcoInstalacion Frontend</h1>
          <p className="subtitle">
            React + Vite client for Electrical Solar Service Management
          </p>

          {/* Dashboard Operacional (Solicitudes y Visitas) */}
          <OperationalDashboardPage />

          {/* Vista Previa de Catálogos (Clientes y Técnicos) */}
          <ApiPreviewPage />
        </div>
        <div className="card-footer">
          <p className="note">
            ℹ️ <strong>Nota del Sprint:</strong> Consumo de APIs en modo de solo lectura. Autenticación y operaciones de escritura pendientes de sprints futuros.
          </p>
        </div>
      </div>
    </div>
  )
}

export default App
