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
import Header from "./components/Header";
import { AuthProvider, useAuth } from "./contexts/AuthContext";
import "./App.css";

function AppRoutes() {
  const { isAuthenticated } = useAuth();

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
          path="/lobby"
          element={isAuthenticated ? <LobbyPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/game/:gameId"
          element={isAuthenticated ? <GamePage /> : <Navigate to="/login" />}
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
