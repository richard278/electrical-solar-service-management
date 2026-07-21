import { useState } from "react";
import { navigationItems, type AppSection } from "./navigation";
import OverviewPage from "../pages/OverviewPage";
import ClientesPage from "../pages/ClientesPage";
import { useAuth } from "../auth/useAuth";
import TecnicosPage from "../pages/TecnicosPage";
import SolicitudesPage from "../pages/SolicitudesPage";
import VisitasTecnicasPage from "../pages/VisitasTecnicasPage";
import { API_BASE_URL } from "../config/environment";

export default function AppShell() {
  const [activeSection, setActiveSection] = useState<AppSection>("overview");
  const { user, logout } = useAuth();

  const renderContent = () => {
    switch (activeSection) {
      case "overview":
        return <OverviewPage />;
      case "clientes":
        return <ClientesPage />;
      case "tecnicos":
        return <TecnicosPage />;
      case "solicitudes":
        return <SolicitudesPage />;
      case "visitas":
        return <VisitasTecnicasPage />;
      default:
        return <OverviewPage />;
    }
  };

  return (
    <div className="app-shell">
      <style>{`
        .sidebar-user-info {
          padding: 12px;
          background: rgba(255, 255, 255, 0.05);
          border-radius: 8px;
          margin-bottom: 16px;
          display: flex;
          flex-direction: column;
          gap: 10px;
          border: 1px solid rgba(255, 255, 255, 0.05);
        }
        .user-details {
          display: flex;
          align-items: center;
          gap: 10px;
        }
        .user-icon {
          font-size: 20px;
          background: rgba(15, 164, 122, 0.15);
          color: #0fa47a;
          padding: 6px;
          border-radius: 50%;
          line-height: 1;
        }
        .user-text {
          display: flex;
          flex-direction: column;
          gap: 2px;
        }
        .user-name {
          font-size: 13px;
          font-weight: 600;
          color: #f1f5f9;
        }
        .user-role {
          font-size: 11px;
          color: #0fa47a;
          font-weight: 500;
          text-transform: uppercase;
          letter-spacing: 0.5px;
        }
        .logout-button {
          width: 100%;
          padding: 8px;
          background: #ef4444;
          color: #ffffff;
          border: none;
          border-radius: 6px;
          font-size: 12px;
          font-weight: 600;
          cursor: pointer;
          transition: background-color 0.2s, transform 0.1s;
          font-family: inherit;
        }
        .logout-button:hover {
          background: #dc2626;
        }
        .logout-button:active {
          transform: scale(0.98);
        }
      `}</style>

      {/* Header Institucional */}
      <header className="app-header">
        <div className="header-brand">
          <div className="brand-logo">☀️</div>
          <div className="brand-info">
            <h1 className="brand-title">EcoInstalacion</h1>
            <span className="brand-subtitle">Electrical Solar Service Management</span>
          </div>
        </div>
        <div className="header-meta">
          <div className="meta-badge sprint-badge">
            Panel operativo React
          </div>
          <div className="meta-badge mode-badge">
            Acceso protegido por roles
          </div>
        </div>
      </header>

      {/* Main Container: Sidebar + Content */}
      <div className="app-container">
        {/* Navigation Sidebar */}
        <aside className="app-sidebar">
          <nav className="sidebar-nav">
            {navigationItems.map((item) => {
              const isActive = activeSection === item.id;
              return (
                <button
                   key={item.id}
                   className={`nav-button ${isActive ? "active" : ""}`}
                   onClick={() => setActiveSection(item.id)}
                   title={item.description}
                >
                  <span className="nav-icon">
                    {item.id === "overview" && "🏠"}
                    {item.id === "clientes" && "👥"}
                    {item.id === "tecnicos" && "🔧"}
                    {item.id === "solicitudes" && "📋"}
                    {item.id === "visitas" && "📐"}
                  </span>
                  <div className="nav-label-container">
                    <span className="nav-label">{item.label}</span>
                    <span className="nav-desc">{item.description}</span>
                  </div>
                </button>
              );
            })}
          </nav>

          {/* Sidebar Footer */}
          <div className="sidebar-footer">
            {user && (
              <div className="sidebar-user-info">
                <div className="user-details">
                  <span className="user-icon">👤</span>
                  <div className="user-text">
                    <span className="user-name">{user.nombre}</span>
                    <span className="user-role">{user.rol}</span>
                  </div>
                </div>
                <button className="logout-button" onClick={logout}>
                  Cerrar sesión
                </button>
              </div>
            )}

            <div className="api-info">
              <span className="api-label">Base API URL:</span>
              <code className="api-url" title={API_BASE_URL}>
                {API_BASE_URL}
              </code>
            </div>
          </div>
        </aside>

        {/* Content Pane */}
        <main className="app-content-pane">
          <div className="page-wrapper">{renderContent()}</div>
        </main>
      </div>
    </div>
  );
}
