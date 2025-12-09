import React from "react";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate } from "react-router-dom";

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const handleProfileClick = () => {
    navigate("/settings");
  };

  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <header className="header">
      <div className="header-content">
        <div className="header-left">
          <h1
            style={{
              color: "#333",
              fontSize: "1.2rem",
              fontWeight: "bold",
              margin: 0,
            }}
          >
            GameImpostors
          </h1>
        </div>

        {user && (
          <div className="header-right">
            <div
              className="user-info"
              onClick={handleProfileClick}
              style={{ cursor: "pointer" }}
            >
              <div className="user-avatar">
                {getInitials(
                  `${
                    user.firstName ||
                    user.lastName ||
                    user.email?.split("@")[0] ||
                    "Utilizator"
                  }`
                )}
              </div>
              <span>
                Bună,{" "}
                {user.firstName ||
                  user.lastName ||
                  user.email?.split("@")[0] ||
                  "Utilizator"}
              </span>
            </div>
            <button 
              className="logout-btn" 
              onClick={handleLogout}
              style={{
                background: "#dc3545",
                color: "white",
                border: "none",
                padding: "8px 16px",
                borderRadius: "20px",
                cursor: "pointer",
                fontWeight: 600,
                fontSize: "14px",
                transition: "all 0.3s ease",
                height: "auto",
                lineHeight: "1.5",
              }}
            >
              Logout
            </button>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;
