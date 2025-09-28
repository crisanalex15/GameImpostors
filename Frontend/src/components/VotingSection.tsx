import React, { useState } from "react";
import { RoundResponse, PlayerResponse } from "../types/game";
import { gameApi } from "../services/api";

export interface VotingSectionProps {
  round: RoundResponse;
  players: PlayerResponse[];
  gameId: string;
  currentUserId?: string;
  onVoteSubmitted: () => void;
}

const VotingSection: React.FC<VotingSectionProps> = ({
  round,
  players,
  currentUserId,
  onVoteSubmitted,
}) => {
  const [selectedPlayer, setSelectedPlayer] = useState("");
  const [reason, setReason] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmitVote = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPlayer || isSubmitting) return;

    setIsSubmitting(true);
    try {
      await gameApi.submitVote(round.id, {
        targetPlayerId: selectedPlayer,
        reason: reason.trim() || undefined,
      });
      onVoteSubmitted();
    } catch (err: any) {
      alert(err.response?.data?.message || "Eroare la trimiterea votului");
    } finally {
      setIsSubmitting(false);
    }
  };

  const getPlayerName = (playerId: string) => {
    // Găsește jucătorul în listă pentru a verifica dacă e utilizatorul curent
    const player = players.find((p) => p.id === playerId);
    if (currentUserId && player && player.userId === currentUserId) {
      return "Tu";
    }
    return `Jucător ${playerId.slice(-4)}`;
  };

  const getPlayerIcon = (player: PlayerResponse) => {
    if (player.isEliminated) return "💀";
    return "👤";
  };

  const canVoteForPlayer = (player: PlayerResponse) => {
    return !player.isEliminated && player.userId !== currentUserId;
  };

  const eligiblePlayers = players.filter(canVoteForPlayer);

  return (
    <div className="card">
      <div className="game-status">
        <h2>🗳️ Faza de Votare</h2>
        <p>Cine crezi că e impostorul?</p>
      </div>

      {!round.hasPlayerVoted ? (
        <form onSubmit={handleSubmitVote}>
          <div className="form-group">
            <label className="form-label">Alege jucătorul suspect:</label>
            <div className="grid grid-2" style={{ marginBottom: "20px" }}>
              {eligiblePlayers.map((player) => (
                <label
                  key={player.id}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    padding: "15px",
                    border:
                      selectedPlayer === player.id
                        ? "2px solid #667eea"
                        : "2px solid #e1e5e9",
                    borderRadius: "8px",
                    cursor: "pointer",
                    background:
                      selectedPlayer === player.id
                        ? "rgba(102, 126, 234, 0.1)"
                        : "white",
                    transition: "all 0.3s ease",
                  }}
                >
                  <input
                    type="radio"
                    name="player"
                    value={player.id}
                    checked={selectedPlayer === player.id}
                    onChange={(e) => setSelectedPlayer(e.target.value)}
                    style={{ marginRight: "10px" }}
                  />
                  <div>
                    <div style={{ fontSize: "1.5rem", marginBottom: "5px" }}>
                      {getPlayerIcon(player)}
                    </div>
                    <div style={{ fontWeight: "bold" }}>
                      {getPlayerName(player.id)}
                    </div>
                    <div style={{ fontSize: "0.9rem", color: "#666" }}>
                      Scor: {player.score}
                    </div>
                  </div>
                </label>
              ))}
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">Motivul votului (opțional):</label>
            <textarea
              className="form-input"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="De ce crezi că acest jucător e impostorul?"
              rows={3}
              style={{ resize: "vertical" }}
            />
          </div>

          <button
            type="submit"
            className="btn btn-danger"
            style={{ width: "100%" }}
            disabled={isSubmitting || !selectedPlayer}
          >
            {isSubmitting ? "Se trimite votul..." : "Trimite Votul"}
          </button>
        </form>
      ) : (
        <div
          style={{
            textAlign: "center",
            padding: "20px",
            background: "rgba(220, 53, 69, 0.1)",
            borderRadius: "8px",
            border: "1px solid rgba(220, 53, 69, 0.3)",
            color: "#dc3545",
            fontWeight: "bold",
          }}
        >
          ✅ Ai votat deja! Așteaptă ca ceilalți să termine de votat.
        </div>
      )}

      {/* Current Votes Display */}
      {round.votes.length > 0 && (
        <div style={{ marginTop: "30px" }}>
          <h3 style={{ textAlign: "center", marginBottom: "20px" }}>
            Voturile curente:
          </h3>
          <div className="grid grid-2">
            {round.votes.map((vote) => (
              <div
                key={vote.id}
                style={{
                  background: "rgba(255, 255, 255, 0.9)",
                  padding: "15px",
                  borderRadius: "8px",
                  border: "1px solid #e1e5e9",
                  marginBottom: "10px",
                }}
              >
                <p style={{ fontWeight: "bold", marginBottom: "5px" }}>
                  {getPlayerName(vote.voterId)} → {getPlayerName(vote.targetId)}
                </p>
                {vote.reason && (
                  <p style={{ fontSize: "0.9rem", color: "#666", margin: 0 }}>
                    "{vote.reason}"
                  </p>
                )}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Voting Instructions */}
      <div
        style={{
          marginTop: "20px",
          padding: "15px",
          background: "rgba(0, 0, 0, 0.05)",
          borderRadius: "8px",
          textAlign: "center",
        }}
      >
        <p style={{ margin: 0, fontSize: "0.9rem", color: "#666" }}>
          💡 Voturile se numără automat. Jucătorul cu cele mai multe voturi va
          fi eliminat.
        </p>
      </div>
    </div>
  );
};

export default VotingSection;
