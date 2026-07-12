import { AuthProvider } from "./auth/AuthContext";
import { useAuth } from "./auth/useAuth";
import AppShell from "./app/AppShell";
import LoginPage from "./pages/LoginPage";
import "./App.css";

function AppContent() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <LoginPage />;
  }

  return <AppShell />;
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
