import { useState } from "react";
import { navigationItems, type AppSection } from "./navigation";
import OverviewPage from "../pages/OverviewPage";
import ClientesPage from "../pages/ClientesPage";
import TecnicosPage from "../pages/TecnicosPage";
import SolicitudesPage from "../pages/SolicitudesPage";
import VisitasTecnicasPage from "../pages/VisitasTecnicasPage";
import { API_BASE_URL } from "../config/environment";

export default function AppShell() {
  const [activeSection, setActiveSection] = useState<AppSection>("overview");

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
            React Navigation Sprint 5F
          </div>
          <div className="meta-badge mode-badge">
            Read-only entity panels
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
