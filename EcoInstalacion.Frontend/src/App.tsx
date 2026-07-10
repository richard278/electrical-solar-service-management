import { API_BASE_URL } from './config/environment'
import './App.css'

function App() {
  return (
    <div className="container">
      <div className="card">
        <div className="card-header">
          <div className="badge">Frontend Scaffold Ready</div>
        </div>
        <div className="card-body">
          <h1 className="title">EcoInstalacion Frontend</h1>
          <p className="subtitle">
            React + Vite client for Electrical Solar Service Management
          </p>
          
          <div className="technical-info">
            <div className="info-item">
              <span className="info-label">Framework</span>
              <span className="info-value">React 18</span>
            </div>
            <div className="info-item">
              <span className="info-label">Language</span>
              <span className="info-value">TypeScript</span>
            </div>
            <div className="info-item">
              <span className="info-label">Build Tool</span>
              <span className="info-value">Vite</span>
            </div>
            <div className="info-item full-width">
              <span className="info-label">Configured API Base URL</span>
              <code className="api-url">{API_BASE_URL}</code>
            </div>
          </div>
        </div>
        <div className="card-footer">
          <p className="note">
            ⚠️ <strong>Nota:</strong> API integration pending Sprint 5C
          </p>
        </div>
      </div>
    </div>
  )
}

export default App
