import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import LobbyPage from "./components/LobbyPage";
import GamePage from "./components/GamePage";
import LoginPage from "./components/LoginPage";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import "./App.css";

function AppRoutes() {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route
        path="/login"
        element={isAuthenticated ? <Navigate to="/lobby" /> : <LoginPage />}
      />
      <Route
        path="/lobby"
        element={isAuthenticated ? <LobbyPage /> : <Navigate to="/login" />}
      />
      <Route
        path="/game/:gameId"
        element={isAuthenticated ? <GamePage /> : <Navigate to="/login" />}
      />
      <Route path="/" element={<Navigate to="/lobby" />} />
    </Routes>
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
