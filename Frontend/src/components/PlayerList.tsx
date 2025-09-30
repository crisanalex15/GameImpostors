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

      <div className="grid grid-3">
        {players.map((player) => {
          const status = getPlayerStatus(player);
          const icon = getPlayerIcon(player);
          const name = getPlayerName(player);

          return (
            <div key={player.id} className={`player-card ${status}`}>
              <div style={{ textAlign: "center" }}>
                <div style={{ fontSize: "2rem", marginBottom: "5px" }}>
                  {icon}
                </div>
                <h3 style={{ marginBottom: "5px" }}>{name}</h3>

                {gameState === GameState.Lobby && (
                  <p
                    style={{
                      color: player.isReady ? "#28a745" : "#6c757d",
                      fontWeight: "bold",
                    }}
                  >
                    {player.isReady ? "Gata" : "Nu e gata"}
                  </p>
                )}

                {gameState === GameState.Game && (
                  <div>
                    <p style={{ color: "#666", fontSize: "0.9rem" }}>
                      Scor: {player.score}
                    </p>
                    {currentRound && (
                      <p
                        style={{
                          color: currentRound.hasPlayerAnswered
                            ? "#28a745"
                            : "#dc3545",
                          fontSize: "0.9rem",
                          fontWeight: "bold",
                        }}
                      >
                        {currentRound.hasPlayerAnswered
                          ? "A răspuns"
                          : "Nu a răspuns"}
                      </p>
                    )}
                  </div>
                )}

                {gameState === GameState.Ended && (
                  <div>
                    <p
                      style={{
                        color: player.isImpostor ? "#dc3545" : "#28a745",
                        fontWeight: "bold",
                      }}
                    >
                      {player.isImpostor ? "Impostor" : "Crewmate"}
                    </p>
                    <p style={{ color: "#666", fontSize: "0.9rem" }}>
                      Scor final: {player.score}
                    </p>
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
