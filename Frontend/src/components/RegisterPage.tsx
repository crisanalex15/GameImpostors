import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
// import { useAuth } from "../contexts/AuthContext"; // Nu este folosit în RegisterPage

const RegisterPage: React.FC = () => {
  const [formData, setFormData] = useState({
    username: "",
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  // const { login } = useAuth(); // Nu este folosit în RegisterPage

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    // Validări de bază
    if (formData.password !== formData.confirmPassword) {
      setError("Parolele nu se potrivesc");
      setIsLoading(false);
      return;
    }

    if (formData.password.length < 6) {
      setError("Parola trebuie să aibă cel puțin 6 caractere");
      setIsLoading(false);
      return;
    }

    try {
      const response = await axios.post(
        "http://localhost:5001/api/Auth/register",
        {
          username: formData.username,
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          password: formData.password,
        }
      );

      if (response.data.message) {
        // Înregistrarea a reușit, dar utilizatorul trebuie să se logheze manual
        setError("Cont creat cu succes! Te poți loga acum.");
        setTimeout(() => {
          navigate("/login");
        }, 2000);
      }
    } catch (err: any) {
      if (err.response?.data?.errors) {
        // Erori de validare din backend
        const errors = err.response.data.errors;
        if (Array.isArray(errors)) {
          setError(errors.join(", "));
        } else {
          setError("Eroare la înregistrare");
        }
      } else if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError("Eroare la înregistrare");
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: "400px", margin: "50px auto" }}>
        <div className="game-status">
          <h1 style={{ color: "#333", marginBottom: "10px" }}>
            🎮 GameImpostors
          </h1>
          <p style={{ color: "#666", fontSize: "1.1rem" }}>Înregistrare</p>
        </div>

        <form onSubmit={handleRegister}>
          <div className="form-group">
            <label className="form-label">Username</label>
            <input
              type="text"
              className="form-input"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              placeholder="Alege un username"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Prenume</label>
            <input
              type="text"
              className="form-input"
              name="firstName"
              value={formData.firstName}
              onChange={handleInputChange}
              placeholder="Introdu prenumele tău"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Nume</label>
            <input
              type="text"
              className="form-input"
              name="lastName"
              value={formData.lastName}
              onChange={handleInputChange}
              placeholder="Introdu numele tău"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Email</label>
            <input
              type="email"
              className="form-input"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              placeholder="Introdu email-ul tău"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Parolă</label>
            <input
              type="password"
              className="form-input"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              placeholder="Introdu parola (min. 6 caractere)"
              required
            />
          </div>

          <div className="form-group">
            <label className="form-label">Confirmă parola</label>
            <input
              type="password"
              className="form-input"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              placeholder="Confirmă parola"
              required
            />
          </div>

          {error && (
            <div
              style={{
                color: error.includes("succes") ? "#28a745" : "#dc3545",
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
            {isLoading ? "Se înregistrează..." : "Înregistrare"}
          </button>

          <button
            type="button"
            className="btn btn-secondary"
            style={{ width: "100%" }}
            onClick={() => navigate("/login")}
          >
            Ai deja cont? Autentifică-te
          </button>
        </form>

        <div style={{ textAlign: "center", marginTop: "20px", color: "#666" }}>
          <p>Prin înregistrare, accepti termenii și condițiile de utilizare.</p>
        </div>
      </div>
    </div>
  );
};

export default RegisterPage;
