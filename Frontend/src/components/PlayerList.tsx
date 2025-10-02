import React from "react";
import { PlayerResponse, GameState, RoundResponse } from "../types/game";

interface PlayerListProps {
  players: PlayerResponse[];
  gameState: GameState;
  currentRound?: RoundResponse;
  currentUserId?: string;
}

const PlayerList: React.FC<PlayerListProps> = ({
  players,
  gameState,
  currentRound,
  currentUserId,
}) => {
  const getPlayerStatus = (player: PlayerResponse) => {
    if (player.isEliminated) return "eliminated";
    if (player.isImpostor && gameState === GameState.Ended) return "impostor";
    if (player.isReady && gameState === GameState.Lobby) return "ready";
    return "normal";
  };

  const getPlayerIcon = (player: PlayerResponse) => {
    if (player.isEliminated) return "💀";
    if (player.isImpostor && gameState === GameState.Ended) return "👹";
    if (player.isReady && gameState === GameState.Lobby) return "✅";
    return "👤";
  };

  const getPlayerName = (player: PlayerResponse) => {
    // Dacă e jucătorul curent, afișează "Tu"
    if (currentUserId && player.userId === currentUserId) {
      return "Tu";
    }
    return `Jucător ${player.userId.slice(-4)}`;
  };

  return (
    <div className="card">
      <h2
        style={{ marginBottom: "8px", textAlign: "center", fontSize: "1.1rem" }}
      >
        Jucători ({players.length})
      </h2>

      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          gap: "10px",
          justifyContent: "center",
          alignItems: "center",
        }}
      >
        {players.map((player) => {
          const status = getPlayerStatus(player);
          const icon = getPlayerIcon(player);
          const name = getPlayerName(player);

          return (
            <div
              key={player.id}
              className={`player-card ${status}`}
              style={{
                display: "flex",
                alignItems: "center",
                gap: "8px",
                padding: "8px 12px",
                borderRadius: "20px",
                backgroundColor:
                  status === "eliminated"
                    ? "#f8f9fa"
                    : status === "impostor"
                    ? "#ffebee"
                    : status === "ready"
                    ? "#e8f5e8"
                    : "#fff",
                border:
                  status === "impostor"
                    ? "2px solid #dc3545"
                    : status === "ready"
                    ? "2px solid #28a745"
                    : "1px solid #ddd",
                minWidth: "fit-content",
                transition: "all 0.3s ease",
              }}
            >
              <div style={{ fontSize: "1.5rem" }}>{icon}</div>
              <div>
                <div
                  style={{
                    fontWeight: "bold",
                    fontSize: "0.9rem",
                    color:
                      status === "eliminated"
                        ? "#6c757d"
                        : status === "impostor"
                        ? "#dc3545"
                        : "#333",
                  }}
                >
                  {name}
                </div>

                {gameState === GameState.Lobby && (
                  <div
                    style={{
                      fontSize: "0.8rem",
                      color: player.isReady ? "#28a745" : "#6c757d",
                      fontWeight: "bold",
                    }}
                  >
                    {player.isReady ? "✓ Gata" : "⏳ Nu e gata"}
                  </div>
                )}

                {gameState === GameState.Game && (
                  <div style={{ fontSize: "0.8rem" }}>
                    <div style={{ color: "#666" }}>Scor: {player.score}</div>
                    {currentRound && (
                      <div
                        style={{
                          color: currentRound.hasPlayerAnswered
                            ? "#28a745"
                            : "#dc3545",
                          fontWeight: "bold",
                        }}
                      >
                        {currentRound.hasPlayerAnswered
                          ? "✓ A răspuns"
                          : "⏳ Nu a răspuns"}
                      </div>
                    )}
                  </div>
                )}

                {gameState === GameState.Ended && (
                  <div style={{ fontSize: "0.8rem" }}>
                    <div
                      style={{
                        color: player.isImpostor ? "#dc3545" : "#28a745",
                        fontWeight: "bold",
                      }}
                    >
                      {player.isImpostor ? "👹 Impostor" : "👤 Crewmate"}
                    </div>
                    <div style={{ color: "#666" }}>Scor: {player.score}</div>
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {gameState === GameState.Lobby && (
        <div
          style={{
            textAlign: "center",
            marginTop: "20px",
            padding: "10px",
            background: "rgba(0, 0, 0, 0.05)",
            borderRadius: "8px",
          }}
        >
          <p style={{ marginBottom: "10px" }}>
            Jucători gata: {players.filter((p) => p.isReady).length} /{" "}
            {players.length}
          </p>
          <p style={{ fontSize: "0.9rem", color: "#666" }}>
            Jocul poate începe când toți jucătorii sunt gata
          </p>
        </div>
      )}
    </div>
  );
};

export default PlayerList;
