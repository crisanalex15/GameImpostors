import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { authApi } from "../services/api";

interface UserProfile {
  id: string;
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  emailConfirmed: boolean;
  isEmailVerified: boolean;
  emailVerifiedAt: string;
  createdAt: string;
  lastModifiedAt: string;
  lastLoginAt: string;
  lockoutEnd: string;
}

interface ApiResponse<T> {
  message?: string;
  error?: string;
  errors?: string[];
  data?: T;
}

const SettingsPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);

  // Form states
  const [usernameForm, setUsernameForm] = useState({ username: "" });
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      setLoading(true);
      const response = await authApi.getMinimalProfile();
      console.log("Profile response:", response);
      // Backend returns data directly, not wrapped in a data property
      setProfile(response);
      // Backend should use camelCase (username with lowercase u) due to JsonNamingPolicy.CamelCase
      setUsernameForm({ username: response.username || "" });
    } catch (error: any) {
      console.error("Profile fetch error:", error);
      showMessage("error", "Failed to fetch profile");
    } finally {
      setLoading(false);
    }
  };

  const showMessage = (type: "success" | "error", text: string) => {
    setMessage({ type, text });
    setTimeout(() => setMessage(null), 5000);
  };

  const handleUpdateUsername = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!usernameForm.username.trim()) {
      showMessage("error", "Username cannot be empty");
      return;
    }

    try {
      setLoading(true);
      await authApi.updateUsername({ username: usernameForm.username });
      showMessage("success", "Username updated successfully");
      fetchProfile(); // Refresh profile
    } catch (error: any) {
      showMessage(
        "error",
        error.response?.data?.message || "Failed to update username"
      );
    } finally {
      setLoading(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();

    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      showMessage("error", "New passwords do not match");
      return;
    }

    if (passwordForm.newPassword.length < 6) {
      showMessage("error", "Password must be at least 6 characters long");
      return;
    }

    try {
      setLoading(true);
      await authApi.changePassword({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
      });
      showMessage("success", "Password changed successfully");
      setPasswordForm({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
      });
    } catch (error: any) {
      showMessage(
        "error",
        error.response?.data?.message || "Failed to change password"
      );
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      await authApi.logout();
      // AuthContext will handle the logout
    } catch (error) {
      console.error("Logout error:", error);
    }
  };

  if (loading && !profile) {
    return (
      <div className="settings-container">
        <div className="loading">Loading profile...</div>
      </div>
    );
  }

  return (
    <div className="settings-container">
      <div className="settings-content">
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            marginBottom: "20px",
          }}
        >
          <h1 style={{ margin: 0 }}>Settings</h1>
          <button
            onClick={() => navigate("/lobby")}
            style={{
              padding: "10px 20px",
              background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
              color: "white",
              border: "none",
              borderRadius: "8px",
              fontSize: "1rem",
              fontWeight: "bold",
              cursor: "pointer",
              transition: "all 0.3s ease",
              display: "flex",
              alignItems: "center",
              gap: "8px",
            }}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = "scale(1.05)";
              e.currentTarget.style.boxShadow =
                "0 4px 15px rgba(102, 126, 234, 0.4)";
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = "scale(1)";
              e.currentTarget.style.boxShadow = "none";
            }}
          >
            ← Back to Lobby
          </button>
        </div>

        {message && (
          <div className={`message ${message.type}`}>{message.text}</div>
        )}

        {/* Profile Information */}
        <div className="settings-section">
          <h2>Profile Information</h2>
          {profile && (
            <div className="profile-info">
              <div className="info-item">
                <label>ID:</label>
                <span>{profile.id}</span>
              </div>
              <div className="info-item">
                <label>Email:</label>
                <span>{profile.email}</span>
              </div>
              <div className="info-item">
                <label>Username:</label>
                <span>{profile.username}</span>
              </div>
              <div className="info-item">
                <label>First Name:</label>
                <span>{profile.firstName}</span>
              </div>
              <div className="info-item">
                <label>Last Name:</label>
                <span>{profile.lastName}</span>
              </div>
              <div className="info-item">
                <label>Phone:</label>
                <span>{profile.phoneNumber || "Not set"}</span>
              </div>
              <div className="info-item">
                <label>Email Verified:</label>
                <span
                  className={
                    profile.isEmailVerified ? "verified" : "not-verified"
                  }
                >
                  {profile.isEmailVerified ? "Yes" : "No"}
                </span>
              </div>
              <div className="info-item">
                <label>Last Login:</label>
                <span>
                  {profile.lastLoginAt
                    ? new Date(profile.lastLoginAt).toLocaleString()
                    : "Never"}
                </span>
              </div>
            </div>
          )}
        </div>

        {/* Update Username */}
        <div className="settings-section">
          <h2>Update Username</h2>
          <form onSubmit={handleUpdateUsername} className="settings-form">
            <div className="form-group">
              <label htmlFor="username">New Username:</label>
              <input
                type="text"
                id="username"
                value={usernameForm.username}
                onChange={(e) => setUsernameForm({ username: e.target.value })}
                placeholder="Enter new username"
                required
              />
            </div>
            <button type="submit" disabled={loading} className="btn-primary">
              {loading ? "Updating..." : "Update Username"}
            </button>
          </form>
        </div>

        {/* Change Password */}
        <div className="settings-section">
          <h2>Change Password</h2>
          <form onSubmit={handleChangePassword} className="settings-form">
            <div className="form-group">
              <label htmlFor="currentPassword">Current Password:</label>
              <input
                type="password"
                id="currentPassword"
                value={passwordForm.currentPassword}
                onChange={(e) =>
                  setPasswordForm({
                    ...passwordForm,
                    currentPassword: e.target.value,
                  })
                }
                placeholder="Enter current password"
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="newPassword">New Password:</label>
              <input
                type="password"
                id="newPassword"
                value={passwordForm.newPassword}
                onChange={(e) =>
                  setPasswordForm({
                    ...passwordForm,
                    newPassword: e.target.value,
                  })
                }
                placeholder="Enter new password"
                required
                minLength={6}
              />
            </div>
            <div className="form-group">
              <label htmlFor="confirmPassword">Confirm New Password:</label>
              <input
                type="password"
                id="confirmPassword"
                value={passwordForm.confirmPassword}
                onChange={(e) =>
                  setPasswordForm({
                    ...passwordForm,
                    confirmPassword: e.target.value,
                  })
                }
                placeholder="Confirm new password"
                required
                minLength={6}
              />
            </div>
            <button type="submit" disabled={loading} className="btn-primary">
              {loading ? "Changing..." : "Change Password"}
            </button>
          </form>
        </div>

        {/* Actions */}
        <div className="settings-section">
          <h2>Account Actions</h2>
          <div className="actions">
            <button onClick={handleLogout} className="btn-danger">
              Logout
            </button>
            <button onClick={fetchProfile} className="btn-secondary">
              Refresh Profile
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SettingsPage;
