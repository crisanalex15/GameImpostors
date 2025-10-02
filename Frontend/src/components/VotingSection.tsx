import React, { useState } from "react";
import { RoundResponse, PlayerResponse, RoundState } from "../types/game";
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

  const getVotingResults = () => {
    if (!round.votes || round.votes.length === 0) return null;

    // Grupează voturile după țintă
    const voteCounts: { [key: string]: number } = {};
    round.votes.forEach((vote) => {
      voteCounts[vote.targetId] = (voteCounts[vote.targetId] || 0) + 1;
    });

    // Găsește jucătorul cu cele mai multe voturi
    const mostVotedPlayer = Object.keys(voteCounts).reduce((a, b) =>
      voteCounts[a] > voteCounts[b] ? a : b
    );

    return {
      mostVotedPlayer,
      voteCounts,
      totalVotes: round.votes.length,
    };
  };

  const getScoreInfo = () => {
    const results = getVotingResults();
    if (!results) return null;

    const mostVotedPlayer = players.find(
      (p) => p.id === results.mostVotedPlayer
    );
    if (!mostVotedPlayer) return null;

    return {
      eliminatedPlayer: mostVotedPlayer,
      wasImpostor: mostVotedPlayer.isImpostor,
      crewmatePoints: mostVotedPlayer.isImpostor ? 100 : 0,
      impostorPoints: mostVotedPlayer.isImpostor ? 50 : 0,
    };
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

      {/* Voting Results and Score Display */}
      {round.state === RoundState.Ended && getScoreInfo() && (
        <div
          style={{
            marginTop: "30px",
            background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
            color: "white",
            padding: "20px",
            borderRadius: "15px",
            textAlign: "center",
          }}
        >
          <h3 style={{ marginBottom: "20px", fontSize: "1.5rem" }}>
            🎯 Rezultatele Votului
          </h3>

          <div
            style={{
              background: "rgba(255, 255, 255, 0.2)",
              padding: "15px",
              borderRadius: "10px",
              marginBottom: "20px",
            }}
          >
            <h4 style={{ marginBottom: "10px" }}>
              {getScoreInfo()?.eliminatedPlayer.isImpostor
                ? "👹 Impostor eliminat!"
                : "👤 Crewmate eliminat"}
            </h4>
            <p style={{ fontSize: "1.1rem", margin: "0" }}>
              {getPlayerName(getScoreInfo()?.eliminatedPlayer.id || "")} a fost
              eliminat
            </p>
          </div>

          <div
            style={{
              background: "rgba(255, 255, 255, 0.2)",
              padding: "15px",
              borderRadius: "10px",
              marginBottom: "20px",
            }}
          >
            <h4 style={{ marginBottom: "15px" }}>🏆 Puncte Acordate</h4>
            <div
              style={{
                display: "flex",
                justifyContent: "space-around",
                flexWrap: "wrap",
              }}
            >
              <div style={{ margin: "10px" }}>
                <div
                  style={{
                    fontSize: "1.2rem",
                    fontWeight: "bold",
                    color: "#28a745",
                  }}
                >
                  +{getScoreInfo()?.crewmatePoints} puncte
                </div>
                <div style={{ fontSize: "0.9rem" }}>Crewmates</div>
              </div>
              {getScoreInfo()?.wasImpostor && (
                <div style={{ margin: "10px" }}>
                  <div
                    style={{
                      fontSize: "1.2rem",
                      fontWeight: "bold",
                      color: "#ffc107",
                    }}
                  >
                    +{getScoreInfo()?.impostorPoints} puncte
                  </div>
                  <div style={{ fontSize: "0.9rem" }}>
                    Impostor (dacă ghiceste cuvântul)
                  </div>
                </div>
              )}
            </div>
          </div>

          {getScoreInfo()?.wasImpostor && (
            <div
              style={{
                background: "rgba(255, 193, 7, 0.3)",
                padding: "15px",
                borderRadius: "10px",
                border: "2px solid #ffc107",
              }}
            >
              <h4 style={{ marginBottom: "10px", color: "#ffc107" }}>
                🎲 Faza Finală - Ghicirea Cuvântului
              </h4>
              <p style={{ margin: "0", fontSize: "1rem" }}>
                Impostorul eliminat poate încerca să ghicească cuvântul pentru a
                câștiga 50 de puncte!
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default VotingSection;
