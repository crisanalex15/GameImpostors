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
          <h1 style={{ 
            color: "#333", 
            fontSize: "1.5rem", 
            fontWeight: "bold",
            margin: 0 
          }}>
            🎮 GameImpostors
          </h1>
        </div>
        
        {user && (
          <div className="header-right">
            <div className="user-info">
              <div className="user-avatar">
                {getInitials(`${user.firstName} ${user.lastName}`)}
              </div>
              <span>
                Bună, {user.firstName}!
              </span>
            </div>
            <button 
              className="logout-btn"
              onClick={handleLogout}
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
