import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { gameApi } from "../services/api";
import { GameStateResponse, GameState, RoundState } from "../types/game";
import PlayerList from "./PlayerList";
import RoundDisplay from "./RoundDisplay";
import VotingSection from "./VotingSection";
import { useAuth } from "../contexts/AuthContext";

const GamePage: React.FC = () => {
  const { gameId } = useParams<{ gameId: string }>();
  const navigate = useNavigate();
  const { user, setCurrentGameId } = useAuth();
  const [gameState, setGameState] = useState<GameStateResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [isReady, setIsReady] = useState(false);
  const [isPlayersExpanded, setIsPlayersExpanded] = useState(false);

  useEffect(() => {
    if (gameId) {
      loadGameState();
      // Poll for game state updates every 2 seconds
      const interval = setInterval(loadGameState, 2000);
      return () => clearInterval(interval);
    }
  }, [gameId]);

  const loadGameState = async () => {
    if (!gameId) return;

    try {
      const state = await gameApi.getGameState(gameId);
      setGameState(state);
      setIsLoading(false);

      // Setează gameId-ul în context pentru a fi redirecționat automat la refresh
      setCurrentGameId(gameId);
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la încărcarea jocului");
      setIsLoading(false);
    }
  };

  const handleSetReady = async () => {
    if (!gameId) return;

    try {
      const newReadyState = !isReady;
      await gameApi.setReady(gameId, newReadyState);
      setIsReady(newReadyState);
      await loadGameState();
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la setarea statusului");
    }
  };

  const handleStartGame = async () => {
    if (!gameId) return;

    try {
      setIsPlayersExpanded(false); // Ascunde lista de jucători când începe jocul
      await gameApi.startGame(gameId);
      await loadGameState();
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la pornirea jocului");
    }
  };

  const togglePlayersList = () => {
    setIsPlayersExpanded(!isPlayersExpanded);
  };

  const handleLeaveGame = async () => {
    if (!gameId) return;

    try {
      await gameApi.leaveGame(gameId);
      setCurrentGameId(null); // Șterge gameId-ul din context
      navigate("/lobby");
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la părăsirea jocului");
    }
  };

  const handleNewGame = async () => {
    if (!gameState) return;

    try {
      // Folosește aceleași setări ca jocul curent
      const newGameData = {
        gameType: gameState.type,
        maxPlayers: gameState.maxPlayers,
        impostorCount: 1, // Default impostor count
        timerDuration: gameState.timerDuration,
        maxRounds: gameState.maxRounds,
      };

      const response = await gameApi.createGame(newGameData);
      if (response.game) {
        setCurrentGameId(response.game.id);
        navigate(`/game/${response.game.id}`);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la crearea jocului nou");
    }
  };

  const handleNextRound = async () => {
    if (!gameId) return;

    console.log("Starting next round for game:", gameId);

    try {
      const response = await gameApi.nextRound(gameId);
      console.log("Next round response:", response);
      if (response.game) {
        setGameState(response.game);
        console.log("Game state updated:", response.game);
      }
    } catch (err: any) {
      console.error("Error starting next round:", err);
      setError(
        err.response?.data?.message || "Eroare la pornirea rundei următoare"
      );
    }
  };

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p style={{ color: "white", fontSize: "1.1rem" }}>
          Se încarcă jocul...
        </p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container">
        <div className="card">
          <div style={{ textAlign: "center", color: "#dc3545" }}>
            <h2>Eroare</h2>
            <p>{error}</p>
            <button
              className="btn btn-primary"
              onClick={() => navigate("/lobby")}
            >
              Înapoi la Lobby
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!gameState) {
    return (
      <div className="container">
        <div className="card">
          <div style={{ textAlign: "center" }}>
            <h2>Jocul nu a fost găsit</h2>
            <button
              className="btn btn-primary"
              onClick={() => navigate("/lobby")}
            >
              Înapoi la Lobby
            </button>
          </div>
        </div>
      </div>
    );
  }

  const isHost = gameState.hostId === user?.id;
  const canStart =
    gameState.state === GameState.Lobby &&
    gameState.players.length >= 3 &&
    gameState.players.every((p) => p.isReady);

  return (
    <div className="container">
      {/* Game Header - Always visible */}
      <div
        className="game-header"
        style={{
          background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
          backdropFilter: "brightness(0.8)",
          width: "100%",
          color: "white",
          padding: "15px",
          borderRadius: "10px",
          borderTopLeftRadius: "0px",
          borderBottomRightRadius: "0px",
          borderBottomLeftRadius: "0px",
          position: "relative",
          border: "1px solid rgba(255, 255, 255, 0.2)",
        }}
      >
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <div>
            <h1
              style={{ fontSize: "1.5rem", marginBottom: "5px", margin: "0" }}
            >
              🎮 GameImpostors
            </h1>
            <p style={{ fontSize: "0.9rem", margin: "5px 0" }}>
              {gameState.state === GameState.Lobby &&
                "Lobby - Așteaptă să înceapă jocul"}
              {gameState.state === GameState.Ended && "Jocul s-a terminat"}
            </p>
            <p style={{ fontSize: "0.9rem", margin: "0" }}>
              Cod: <strong>{gameState.lobbyCode}</strong>
              {gameState.state === GameState.Game && (
                <span style={{ marginLeft: "15px" }}>
                  👥 {gameState.players.length} jucători
                </span>
              )}
            </p>
            {user && (
              <p
                style={{
                  fontSize: "0.8rem",
                  margin: "5px 0 0 0",
                  opacity: 0.9,
                }}
              >
                👤 {user.userName || user.email?.split("@")[0] || "Utilizator"}
              </p>
            )}
          </div>

          {/* Header Buttons - Show in lobby, during game, and when ended */}
          {(gameState.state === GameState.Game ||
            gameState.state === GameState.Lobby ||
            gameState.state === GameState.Ended) && (
            <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
              {/* Players Toggle Button - Only show during game rounds */}
              {gameState.state === GameState.Game && (
                <button
                  onClick={togglePlayersList}
                  style={{
                    background: "rgba(255,255,255,0.2)",
                    border: "none",
                    borderRadius: "50%",
                    width: "50px",
                    height: "50px",
                    color: "white",
                    fontSize: "1.2rem",
                    cursor: "pointer",
                    transition: "all 0.3s ease",
                    transform: isPlayersExpanded
                      ? "rotate(180deg)"
                      : "rotate(0deg)",
                  }}
                  title={
                    isPlayersExpanded ? "Ascunde jucătorii" : "Arată jucătorii"
                  }
                >
                  ⬇️
                </button>
              )}

              {/* Leave Game Button - Show in lobby, during game, and when ended */}
              {(gameState.state === GameState.Game ||
                gameState.state === GameState.Lobby ||
                gameState.state === GameState.Ended) && (
                <button
                  onClick={handleLeaveGame}
                  style={{
                    background: "rgba(220, 53, 69, 0.8)",
                    border: "none",
                    borderRadius: "50%",
                    width: "50px",
                    height: "50px",
                    color: "white",
                    fontSize: "1.2rem",
                    cursor: "pointer",
                    transition: "all 0.3s ease",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                  }}
                  title="Părăsește Jocul"
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = "rgba(220, 53, 69, 1)";
                    e.currentTarget.style.transform = "scale(1.1)";
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = "rgba(220, 53, 69, 0.8)";
                    e.currentTarget.style.transform = "scale(1)";
                  }}
                >
                  🚪
                </button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Players List - Collapsible during game */}
      <div
        style={{
          maxHeight:
            gameState.state === GameState.Lobby
              ? "none"
              : isPlayersExpanded
              ? "500px"
              : "0px",
          overflow: "hidden",
          transition: "max-height 0.3s ease",
          marginBottom: "0px", // Eliminat margin-ul
        }}
      >
        <div
          className="card"
          style={{
            padding: "15px",
            borderTopLeftRadius: "0px",
            borderTopRightRadius: "0px",
            margin: "0", // Eliminat orice margin
            borderBottomLeftRadius: "0px",
            borderBottomRightRadius: "0px",
          }}
        >
          {/* Game Controls */}
          {gameState.state === GameState.Lobby && (
            <div
              style={{
                textAlign: "center",
                marginBottom: "10px",
              }}
            >
              <button
                className={`btn ${isReady ? "btn-success" : "btn-primary"}`}
                onClick={handleSetReady}
                style={{
                  marginRight: "10px",
                  minWidth: "150px", // Lățime fixă
                  height: "40px", // Înălțime fixă
                  transition: "all 0.3s ease",
                }}
              >
                {isReady ? "✓ Gata" : "Nu sunt gata"}
              </button>

              {isHost && (
                <button
                  className="btn btn-start"
                  onClick={handleStartGame}
                  disabled={!canStart}
                >
                  {canStart ? "Începe Jocul" : "Nu toți jucătorii sunt gata"}
                </button>
              )}
            </div>
          )}
          <PlayerList
            players={gameState.players}
            gameState={gameState.state}
            currentRound={gameState.currentRound}
            currentUserId={user?.id}
          />
        </div>
      </div>

      {/* Current Round Display - Always visible */}
      {gameState.currentRound && (
        <RoundDisplay
          round={gameState.currentRound}
          gameType={gameState.type}
          gameId={gameId!}
          hostId={gameState.hostId}
          currentUserId={user?.id}
          currentRoundNumber={gameState.roundNumber}
          maxRounds={gameState.maxRounds}
          players={gameState.players}
          onStateUpdate={loadGameState}
          onNewGame={handleNewGame}
          onLeaveLobby={handleLeaveGame}
          onNextRound={handleNextRound}
        />
      )}

      {/* Voting Section - Show during Voting and Ended round states (but not game ended) */}
      {gameState.state !== GameState.Ended &&
        gameState.currentRound &&
        (gameState.currentRound.state === RoundState.Voting ||
          gameState.currentRound.state === RoundState.Ended) && (
          <VotingSection
            round={gameState.currentRound}
            players={gameState.players}
            gameId={gameId!}
            currentUserId={user?.id}
            gameType={gameState.type}
            onVoteSubmitted={loadGameState}
            onNextRound={handleNextRound}
          />
        )}

      {/* Final Leaderboard - Show when game is ended */}
      {gameState.state === GameState.Ended && (
        <div
          style={{
            textAlign: "center",
            padding: "30px",
            background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
            color: "white",
            borderRadius: "15px",
            border: "2px solid rgba(255, 255, 255, 0.3)",
            boxShadow: "0 8px 32px rgba(0, 0, 0, 0.3)",
            width: "100%",
            borderTopLeftRadius: "0px",
            borderTopRightRadius: "0px",
          }}
        >
          <h2 style={{ marginBottom: "30px", fontSize: "2rem" }}>
            🏆 Jocul S-a Terminat!
          </h2>

          {/* Final Leaderboard */}
          <div style={{ marginBottom: "30px" }}>
            {gameState.players
              .sort((a, b) => (b.score || 0) - (a.score || 0))
              .map((player, index) => (
                <div
                  key={player.id}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    padding: "15px 25px",
                    margin: "10px 0",
                    background:
                      index === 0
                        ? "rgba(255, 215, 0, 0.3)"
                        : "rgba(255, 255, 255, 0.2)",
                    borderRadius: "12px",
                    border:
                      index === 0
                        ? "2px solid #ffd700"
                        : "1px solid rgba(255, 255, 255, 0.3)",
                  }}
                >
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                      gap: "15px",
                    }}
                  >
                    <div
                      style={{
                        fontSize: "1.5rem",
                        fontWeight: "bold",
                        color:
                          index === 0
                            ? "#ffd700"
                            : index === 1
                            ? "#c0c0c0"
                            : index === 2
                            ? "#cd7f32"
                            : "white",
                      }}
                    >
                      {index === 0
                        ? "🥇"
                        : index === 1
                        ? "🥈"
                        : index === 2
                        ? "🥉"
                        : `${index + 1}.`}
                    </div>
                    <div style={{ textAlign: "left" }}>
                      <div style={{ fontWeight: "bold", fontSize: "1.2rem" }}>
                        {player.userName || `Jucător ${player.id.slice(-4)}`}
                      </div>
                    </div>
                  </div>
                  <div
                    style={{
                      fontSize: "1.5rem",
                      fontWeight: "bold",
                      color: index === 0 ? "#ffd700" : "white",
                    }}
                  >
                    {player.score || 0} pts
                  </div>
                </div>
              ))}
          </div>

          {/* Action Buttons */}
          <div
            style={{
              display: "flex",
              gap: "15px",
              justifyContent: "center",
              flexWrap: "wrap",
            }}
          >
            {user?.id === gameState.hostId && (
              <button
                onClick={handleNewGame}
                style={{
                  padding: "15px 30px",
                  fontSize: "1.1rem",
                  background: "rgba(40, 167, 69, 0.9)",
                  color: "white",
                  border: "none",
                  borderRadius: "10px",
                  cursor: "pointer",
                  fontWeight: "bold",
                  transition: "all 0.3s ease",
                }}
                onMouseEnter={(e) =>
                  (e.currentTarget.style.background = "rgba(40, 167, 69, 1)")
                }
                onMouseLeave={(e) =>
                  (e.currentTarget.style.background = "rgba(40, 167, 69, 0.9)")
                }
              >
                🎮 Meci Nou
              </button>
            )}
            <button
              onClick={handleLeaveGame}
              style={{
                padding: "15px 30px",
                fontSize: "1.1rem",
                background: "rgba(220, 53, 69, 0.9)",
                color: "white",
                border: "none",
                borderRadius: "10px",
                cursor: "pointer",
                fontWeight: "bold",
                transition: "all 0.3s ease",
              }}
              onMouseEnter={(e) =>
                (e.currentTarget.style.background = "rgba(220, 53, 69, 1)")
              }
              onMouseLeave={(e) =>
                (e.currentTarget.style.background = "rgba(220, 53, 69, 0.9)")
              }
            >
              🚪 Părăsește Lobby
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default GamePage;
