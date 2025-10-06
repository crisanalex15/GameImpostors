import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import LobbyPage from "./components/LobbyPage";
import GamePage from "./components/GamePage";
import LoginPage from "./components/LoginPage";
import RegisterPage from "./components/RegisterPage";
import SettingsPage from "./components/SettingsPage";
import ForgotPasswordPage from "./components/ForgotPasswordPage";
import Header from "./components/Header";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import { useGameRedirect } from "./hooks/useGameRedirect";
import "./App.css";

function AppRoutes() {
  const { isAuthenticated } = useAuth();

  // Verifică dacă utilizatorul este într-un joc activ și îl redirecționează
  useGameRedirect();

  return (
    <>
      {isAuthenticated && <Header />}
      <Routes>
        <Route
          path="/login"
          element={isAuthenticated ? <Navigate to="/lobby" /> : <LoginPage />}
        />
        <Route
          path="/register"
          element={
            isAuthenticated ? <Navigate to="/lobby" /> : <RegisterPage />
          }
        />
        <Route
          path="/forgot-password"
          element={
            isAuthenticated ? <Navigate to="/lobby" /> : <ForgotPasswordPage />
          }
        />
        <Route
          path="/lobby"
          element={isAuthenticated ? <LobbyPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/game/:gameId"
          element={isAuthenticated ? <GamePage /> : <Navigate to="/login" />}
        />
        <Route
          path="/settings"
          element={
            isAuthenticated ? <SettingsPage /> : <Navigate to="/login" />
          }
        />
        <Route path="/" element={<Navigate to="/lobby" />} />
      </Routes>
    </>
  );
}

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <AppRoutes />
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
