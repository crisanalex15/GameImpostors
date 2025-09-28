import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import { useAuth } from "../contexts/AuthContext";

const LoginPage: React.FC = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    try {
      const response = await axios.post(
        "http://localhost:5086/api/Auth/login",
        {
          email,
          password,
        }
      );

      if (response.data.token) {
        login(response.data.token, response.data.user);
        navigate("/lobby");
      } else {
        setError("Token de autentificare lipsă");
      }
    } catch (err: any) {
      if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError("Eroare la autentificare");
      }
    } finally {
      setIsLoading(false);
    }
  };

  // const handleDemoLogin = () => {
  //   localStorage.setItem("authToken", "demo-token");
  //   navigate("/lobby");
  // };

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: "400px", margin: "100px auto" }}>
        <div className="game-status">
          <h1 style={{ color: "#333", marginBottom: "10px" }}>
            🎮 GameImpostors
          </h1>
          <p style={{ color: "#666", fontSize: "1.1rem" }}>Autentificare</p>
        </div>

        <form onSubmit={handleLogin}>
          <div className="form-group">
            <label className="form-label">Email</label>
            <input
              type="email"
              className="form-input"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Introdu email-ul tău"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Parolă</label>
            <input
              type="password"
              className="form-input"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Introdu parola"
              required
            />
          </div>

          {error && (
            <div
              style={{
                color: "#dc3545",
                marginBottom: "20px",
                textAlign: "center",
              }}
            >
              {error}
            </div>
          )}

          <button
            type="submit"
            className="btn btn-primary"
            style={{ width: "100%", marginBottom: "15px" }}
            disabled={isLoading}
          >
            {isLoading ? "Se autentifică..." : "Autentificare"}
          </button>
        </form>

        <div style={{ textAlign: "center", marginTop: "20px", color: "#666" }}>
          <p>
            Nu ai cont?{" "}
            <button
              type="button"
              onClick={() => navigate("/register")}
              style={{
                background: "none",
                border: "none",
                color: "#667eea",
                textDecoration: "underline",
                cursor: "pointer",
                fontSize: "inherit",
              }}
            >
              Înregistrează-te aici
            </button>
          </p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
