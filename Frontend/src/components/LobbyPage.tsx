import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { gameApi } from "../services/api";
import { CreateGameRequest, GameType } from "../types/game";
import { useAuth } from "../contexts/AuthContext";

const LobbyPage: React.FC = () => {
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [showJoinForm, setShowJoinForm] = useState(false);
  const [lobbyCode, setLobbyCode] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { user, setCurrentGameId } = useAuth();

  const [createGameData, setCreateGameData] = useState<CreateGameRequest>({
    gameType: GameType.WordHidden,
    maxPlayers: 6,
    impostorCount: 1,
    timerDuration: 120,
    maxRounds: 3,
  });

  const handleCreateGame = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    try {
      const response = await gameApi.createGame(createGameData);
      if (response.game) {
        setCurrentGameId(response.game.id); // Setează gameId-ul în context
        navigate(`/game/${response.game.id}`);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la crearea jocului");
    } finally {
      setIsLoading(false);
    }
  };

  const handleJoinGame = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError("");

    try {
      const response = await gameApi.joinGame({ lobbyCode });
      if (response.game) {
        setCurrentGameId(response.game.id); // Setează gameId-ul în context
        navigate(`/game/${response.game.id}`);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la alăturarea la joc");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container">
      <div className="card">
        <div className="game-status">
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <div>
              <h1 style={{ color: "#333", marginBottom: "10px" }}>
                🎮 GameImpostors
              </h1>
              <p style={{ color: "#666", fontSize: "1.1rem" }}>
                Bine ai venit în lobby,{" "}
                <strong>
                  {user?.firstName ||
                    user?.lastName ||
                    user?.email?.split("@")[0] ||
                    "Utilizator"}
                </strong>
                !
              </p>
            </div>
          </div>
        </div>

        <div className="grid grid-2" style={{ marginTop: "40px" }}>
          {/* Create Game */}
          <div className="card">
            <h2 style={{ marginBottom: "20px", textAlign: "center" }}>
              Creează Joc Nou
            </h2>

            {!showCreateForm ? (
              <button
                className="btn btn-primary"
                style={{
                  width: "100%",
                  transition: "all 0.3s ease",
                  transform: "scale(1)",
                }}
                onClick={() => setShowCreateForm(true)}
                onMouseEnter={(e) =>
                  (e.currentTarget.style.transform = "scale(1.02)")
                }
                onMouseLeave={(e) =>
                  (e.currentTarget.style.transform = "scale(1)")
                }
              >
                Creează Joc
              </button>
            ) : (
              <form
                onSubmit={handleCreateGame}
                className="fade-in"
                style={{
                  animation: "slideIn 0.4s ease-out",
                }}
              >
                <div className="form-group">
                  <label className="form-label">Tip Joc</label>
                  <select
                    className="form-select"
                    value={createGameData.gameType}
                    onChange={(e) =>
                      setCreateGameData({
                        ...createGameData,
                        gameType: parseInt(e.target.value) as GameType,
                      })
                    }
                  >
                    <option value={GameType.WordHidden}>Word Hidden</option>
                    <option value={GameType.Questions}>Question Swap</option>
                  </select>
                </div>

                <div className="form-group">
                  <label className="form-label">
                    Numărul maxim de jucători
                  </label>
                  <select
                    className="form-select"
                    value={createGameData.maxPlayers}
                    onChange={(e) =>
                      setCreateGameData({
                        ...createGameData,
                        maxPlayers: parseInt(e.target.value),
                      })
                    }
                  >
                    <option value={3}>3 jucători</option>
                    <option value={4}>4 jucători</option>
                    <option value={5}>5 jucători</option>
                    <option value={6}>6 jucători</option>
                    <option value={8}>8 jucători</option>
                    <option value={10}>10 jucători</option>
                  </select>
                </div>

                <div className="form-group">
                  <label className="form-label">Numărul de impostori</label>
                  <select
                    className="form-select"
                    value={createGameData.impostorCount}
                    onChange={(e) =>
                      setCreateGameData({
                        ...createGameData,
                        impostorCount: parseInt(e.target.value),
                      })
                    }
                  >
                    <option value={1}>1 impostor</option>
                    <option value={2}>2 impostori</option>
                    <option value={3}>3 impostori</option>
                    <option value={4}>Random</option>
                  </select>
                </div>

                <div className="form-group">
                  <label className="form-label">Durata rundei (secunde)</label>
                  <select
                    className="form-select"
                    value={createGameData.timerDuration}
                    onChange={(e) =>
                      setCreateGameData({
                        ...createGameData,
                        timerDuration: parseInt(e.target.value),
                      })
                    }
                  >
                    <option value={60}>1 minut</option>
                    <option value={120}>2 minute</option>
                    <option value={180}>3 minute</option>
                    <option value={300}>5 minute</option>
                  </select>
                </div>

                {/* <div className="form-group">
                  <label className="form-label">Numărul de runde</label>
                  <select
                    className="form-select"
                    value={createGameData.maxRounds}
                    onChange={(e) =>
                      setCreateGameData({
                        ...createGameData,
                        maxRounds: parseInt(e.target.value),
                      })
                    }
                  >
                    <option value={1}>1 rundă</option>
                    <option value={2}>2 runde</option>
                    <option value={3}>3 runde</option>
                    <option value={5}>5 runde</option>
                  </select>
                </div> */}

                <button
                  type="submit"
                  className="btn btn-success"
                  disabled={isLoading}
                >
                  {isLoading ? "Se creează..." : "Creează Joc"}
                </button>

                <button
                  type="button"
                  className="btn btn-secondary"
                  style={{
                    transition: "all 0.3s ease",
                    transform: "scale(1)",
                  }}
                  onClick={() => setShowCreateForm(false)}
                  onMouseEnter={(e) =>
                    (e.currentTarget.style.transform = "scale(1.02)")
                  }
                  onMouseLeave={(e) =>
                    (e.currentTarget.style.transform = "scale(1)")
                  }
                >
                  Anulează
                </button>
              </form>
            )}
          </div>

          {/* Join Game */}
          {!showCreateForm && (
            <div className="card">
              <h2 style={{ marginBottom: "20px", textAlign: "center" }}>
                Alătură-te la Joc
              </h2>

              {!showJoinForm ? (
                <button
                  className="btn btn-primary"
                  style={{
                    width: "100%",
                    transition: "all 0.3s ease",
                    transform: "scale(1)",
                  }}
                  onClick={() => setShowJoinForm(true)}
                  onMouseEnter={(e) =>
                    (e.currentTarget.style.transform = "scale(1.02)")
                  }
                  onMouseLeave={(e) =>
                    (e.currentTarget.style.transform = "scale(1)")
                  }
                >
                  Alătură-te la Joc
                </button>
              ) : (
                <form
                  onSubmit={handleJoinGame}
                  className="fade-in"
                  style={{
                    animation: "slideIn 0.4s ease-out",
                  }}
                >
                  <div className="form-group">
                    <label className="form-label">Cod Lobby</label>
                    <input
                      type="text"
                      className="form-input"
                      value={lobbyCode}
                      onChange={(e) =>
                        setLobbyCode(e.target.value.toUpperCase())
                      }
                      placeholder="Introdu codul lobby-ului"
                      maxLength={6}
                      style={{
                        textAlign: "center",
                        fontSize: "18px",
                        letterSpacing: "2px",
                      }}
                      required
                    />
                  </div>

                  <button
                    type="submit"
                    className="btn btn-success"
                    style={{ width: "100%", marginBottom: "10px" }}
                    disabled={isLoading || lobbyCode.length !== 6}
                  >
                    {isLoading ? "Se alătură..." : "Alătură-te"}
                  </button>

                  <button
                    type="button"
                    className="btn btn-secondary"
                    style={{
                      width: "100%",
                      transition: "all 0.3s ease",
                      transform: "scale(1)",
                    }}
                    onClick={() => {
                      setShowJoinForm(false);
                      setLobbyCode("");
                    }}
                    onMouseEnter={(e) =>
                      (e.currentTarget.style.transform = "scale(1.02)")
                    }
                    onMouseLeave={(e) =>
                      (e.currentTarget.style.transform = "scale(1)")
                    }
                  >
                    Anulează
                  </button>
                </form>
              )}
            </div>
          )}
        </div>

        {error && (
          <div
            style={{
              color: "#dc3545",
              textAlign: "center",
              marginTop: "20px",
              padding: "15px",
              background: "rgba(220, 53, 69, 0.1)",
              borderRadius: "8px",
              border: "1px solid rgba(220, 53, 69, 0.3)",
            }}
          >
            {error}
          </div>
        )}
      </div>
    </div>
  );
};

export default LobbyPage;
