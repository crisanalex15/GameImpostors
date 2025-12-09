import React, { useState } from "react";
import {
  RoundResponse,
  PlayerResponse,
  RoundState,
  GameType,
} from "../types/game";
import { gameApi } from "../services/api";

export interface VotingSectionProps {
  round: RoundResponse;
  players: PlayerResponse[];
  gameId: string;
  currentUserId?: string;
  gameType?: GameType;
  onVoteSubmitted: () => void;
  onNextRound?: () => void;
}

const VotingSection: React.FC<VotingSectionProps> = ({
  round,
  players,
  gameId,
  currentUserId,
  gameType,
  onVoteSubmitted,
}) => {
  const [selectedPlayer, setSelectedPlayer] = useState("");
  const [reason] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [wordGuess, setWordGuess] = useState("");
  const [isGuessing, setIsGuessing] = useState(false);
  const [hasGuessed, setHasGuessed] = useState(false); // Track if impostor has already guessed
  const [guessResult, setGuessResult] = useState<{
    success: boolean;
    message: string;
    points: number;
  } | null>(null);
  const [isReadyForNextRound, setIsReadyForNextRound] = useState(false);

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

  const handleWordGuess = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!wordGuess.trim() || isGuessing || hasGuessed) return;

    setIsGuessing(true);
    setHasGuessed(true); // Mark that impostor has used their guess attempt
    try {
      const result = await gameApi.guessWord(round.id, wordGuess.trim());
      setGuessResult({
        success: true,
        message: result.message,
        points: result.points,
      });
      setWordGuess("");
      onVoteSubmitted(); // Actualizează starea jocului
    } catch (err: any) {
      setGuessResult({
        success: false,
        message: err.response?.data?.message || "Eroare la ghicirea cuvântului",
        points: 0,
      });
    } finally {
      setIsGuessing(false);
    }
  };

  // Verifică dacă jocul este de tip WordHidden pentru ghicirea cuvântului
  const canGuessWord = gameType === GameType.WordHidden;

  const getPlayerName = (playerId: string) => {
    const player = players.find((p) => p.id === playerId);
    return player?.userName || `Jucător ${playerId.slice(-4)}`;
  };

  const isCurrentUserEliminatedImpostor = () => {
    if (!currentUserId || !getScoreInfo()?.eliminatedPlayer.isImpostor)
      return false;

    const eliminatedPlayer = players.find(
      (p) => p.id === getScoreInfo()?.eliminatedPlayer.id
    );
    return eliminatedPlayer && eliminatedPlayer.userId === currentUserId;
  };

  const getPlayerUserName = (playerId: string) => {
    const player = players.find((p) => p.id === playerId);
    return player?.userName || getPlayerName(playerId);
  };

  const getPlayerIcon = (player: PlayerResponse) => {
    if (player.isEliminated) return "💀";
    return "";
  };

  const getReadyForNextRoundCount = () => {
    // Count all players who are ready (including eliminated ones who are spectating)
    const readyPlayers = players.filter((p) => p.isReady);
    const totalPlayers = players.length; // All players, including eliminated
    return { ready: readyPlayers.length, total: totalPlayers };
  };

  const handleReadyForNextRound = async () => {
    if (isReadyForNextRound) return; // Already ready

    setIsReadyForNextRound(true);

    try {
      // Set player as ready
      const response = await gameApi.setReady(gameId, true);
      console.log("Player set as ready, updated game state:", response);

      // The polling in GamePage will detect when all players are ready
      // and automatically trigger the next round
    } catch (error) {
      console.error("Error setting ready for next round:", error);
      setIsReadyForNextRound(false);
    }
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
      crewmatePoints: mostVotedPlayer.isImpostor ? 100 : 0, // Crewmates get 100 if impostor was voted
      impostorPoints: mostVotedPlayer.isImpostor ? 0 : 100, // Impostor gets 0 if voted, 100 if crewmate was voted
    };
  };

  const canVoteForPlayer = (player: PlayerResponse) => {
    return !player.isEliminated && player.userId !== currentUserId;
  };

  const eligiblePlayers = players.filter(canVoteForPlayer);

  return (
    <div
      className="card"
      style={{
        borderTopLeftRadius: "0px",
        borderTopRightRadius: "0px",
        borderBottomLeftRadius: "12px",
        borderBottomRightRadius: "12px",
      }}
    >
      <div className="game-status">
        <h2>Faza de Votare</h2>
        <p>Cine crezi că e impostorul?</p>
      </div>

      {!round.hasPlayerVoted ? (
        <form onSubmit={handleSubmitVote}>
          <div className="form-group">
            <label className="form-label ">Alege jucătorul suspect:</label>
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
                    <div style={{ fontWeight: "bold", fontSize: "1.1rem" }}>
                      {getPlayerIcon(player) && (
                        <span style={{ marginRight: "5px" }}>
                          {getPlayerIcon(player)}
                        </span>
                      )}
                      {getPlayerUserName(player.id)}
                    </div>
                    <div
                      style={{
                        fontSize: "0.9rem",
                        color: "#666",
                        marginTop: "5px",
                      }}
                    >
                      Scor: {player.score}
                    </div>
                  </div>
                </label>
              ))}
            </div>
          </div>

          {/* <div className="form-group">
            <label className="form-label">Motivul votului (opțional):</label>
            <textarea
              className="form-input"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="De ce crezi că acest jucător e impostorul?"
              rows={3}
              style={{ resize: "vertical" }}
            />
          </div> */}

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
            background: "rgba(255, 255, 255, 0.1)",
            borderRadius: "8px",
            border: "1px solid rgba(220, 53, 69, 0.3)",
            color: "#666",
            fontWeight: "bold",
          }}
        >
          Ai votat, așteaptă ca ceilalți să termine de votat.
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
                  {getPlayerUserName(vote.voterId)} →{" "}
                  {getPlayerUserName(vote.targetId)}
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

      {/* Voting Results and Score Display */}
      {round.state === RoundState.Ended && getScoreInfo() && (
        <div
          style={{
            marginTop: "30px",
            background: "white",
            color: "#333",
            padding: "25px",
            borderRadius: "12px",
            border: "1px solid #e1e5e9",
          }}
        >
          <h3
            style={{
              marginBottom: "20px",
              fontSize: "1.5rem",
              color: "#333",
              textAlign: "center",
            }}
          >
            Rezultatele Votului
          </h3>

          <div
            style={{
              background: getScoreInfo()?.eliminatedPlayer.isImpostor
                ? "rgba(40, 167, 69, 0.1)"
                : "rgba(220, 53, 69, 0.1)",
              padding: "15px",
              borderRadius: "10px",
              marginBottom: "20px",
              border: `2px solid ${
                getScoreInfo()?.eliminatedPlayer.isImpostor
                  ? "#28a745"
                  : "#dc3545"
              }`,
            }}
          >
            <h4
              style={{
                marginBottom: "10px",
                fontSize: "1.2rem",
                color: "#333",
              }}
            >
              {getScoreInfo()?.eliminatedPlayer.isImpostor
                ? "Impostor găsit!"
                : "Crewmate votat (greșit)"}
            </h4>
            <p style={{ fontSize: "1rem", margin: "0", color: "#666" }}>
              {getPlayerUserName(getScoreInfo()?.eliminatedPlayer.id || "")} (
              {getScoreInfo()?.eliminatedPlayer.isImpostor
                ? "Impostor"
                : "Crewmate"}
              ) a fost votat
            </p>
          </div>

          <div
            style={{
              background: "#f8f9fa",
              padding: "15px",
              borderRadius: "10px",
              marginBottom: "20px",
              border: "1px solid #e1e5e9",
            }}
          >
            <h4
              style={{
                marginBottom: "15px",
                fontSize: "1.2rem",
                color: "#333",
              }}
            >
              Puncte Acordate
            </h4>
            <div
              style={{
                display: "flex",
                justifyContent: "space-around",
                flexWrap: "wrap",
              }}
            >
              {/* Show crewmates points if impostor was voted */}
              {getScoreInfo()?.wasImpostor && (
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
                </div>
              )}

              {/* Show points when crewmate was voted */}
              {!getScoreInfo()?.wasImpostor &&
                (() => {
                  const currentPlayer = players.find(
                    (p) => p.userId === currentUserId
                  );
                  const isCurrentUserImpostor = currentPlayer?.isImpostor;

                  if (isCurrentUserImpostor) {
                    // Impostor gets 100 points
                    return (
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
                      </div>
                    );
                  } else {
                    // Crewmate gets 0 points (they voted wrong)
                    return (
                      <div style={{ margin: "10px" }}>
                        <div
                          style={{
                            fontSize: "1.2rem",
                            fontWeight: "bold",
                            color: "#dc3545",
                          }}
                        >
                          +0 puncte
                        </div>
                      </div>
                    );
                  }
                })()}
            </div>
          </div>

          {getScoreInfo()?.wasImpostor && canGuessWord && (
            <div
              style={{
                background: "rgba(255, 193, 7, 0.3)",
                padding: "15px",
                borderRadius: "10px",
                border: "2px solid #ffc107",
              }}
            >
              <h4
                style={{
                  marginBottom: "10px",
                  color: "#ffc107",
                  fontSize: "1.2rem",
                }}
              >
                Faza Finală - Ghicirea Cuvântului
              </h4>
              <p style={{ margin: "0 0 15px 0", fontSize: "1rem" }}>
                Impostorul votat poate încerca să ghicească cuvântul pentru a
                câștiga 50 de puncte!
              </p>

              {isCurrentUserEliminatedImpostor() &&
                canGuessWord &&
                !hasGuessed && (
                  <form
                    onSubmit={handleWordGuess}
                    style={{ marginTop: "15px" }}
                  >
                    <div
                      style={{
                        display: "flex",
                        gap: "10px",
                        alignItems: "center",
                      }}
                    >
                      <input
                        type="text"
                        value={wordGuess}
                        onChange={(e) => setWordGuess(e.target.value)}
                        placeholder="Introdu cuvântul... (o singură încercare!)"
                        style={{
                          flex: 1,
                          padding: "10px",
                          borderRadius: "5px",
                          border: "1px solid #ffc107",
                          fontSize: "1rem",
                          background: "rgba(255, 255, 255, 0.9)",
                        }}
                        disabled={isGuessing}
                      />
                      <button
                        type="submit"
                        disabled={!wordGuess.trim() || isGuessing}
                        style={{
                          padding: "10px 20px",
                          background: isGuessing ? "#6c757d" : "#ffc107",
                          color: "white",
                          border: "none",
                          borderRadius: "5px",
                          fontSize: "1rem",
                          fontWeight: "bold",
                          cursor: isGuessing ? "not-allowed" : "pointer",
                          transition: "all 0.3s ease",
                        }}
                      >
                        {isGuessing ? "Se procesează..." : "Ghicește!"}
                      </button>
                    </div>
                  </form>
                )}

              {/* Show message if impostor has already guessed */}
              {isCurrentUserEliminatedImpostor() &&
                canGuessWord &&
                hasGuessed &&
                !guessResult && (
                  <div
                    style={{
                      marginTop: "15px",
                      padding: "15px",
                      borderRadius: "8px",
                      background: "rgba(108, 117, 125, 0.2)",
                      border: "2px solid #6c757d",
                      color: "white",
                      textAlign: "center",
                    }}
                  >
                    <p style={{ margin: 0, fontSize: "1rem" }}>
                      ⏳ Așteptăm rezultatul...
                    </p>
                  </div>
                )}

              {/* Rezultatul ghicirii */}
              {guessResult && (
                <div
                  style={{
                    marginTop: "15px",
                    padding: "15px",
                    borderRadius: "8px",
                    background: guessResult.success
                      ? "rgba(40, 167, 69, 0.2)"
                      : "rgba(220, 53, 69, 0.2)",
                    border: `2px solid ${
                      guessResult.success ? "#28a745" : "#dc3545"
                    }`,
                    color: "white",
                  }}
                >
                  <div style={{ fontSize: "1.2rem", fontWeight: "bold" }}>
                    {guessResult.success ? "Corect!" : "Greșit!"}
                  </div>
                  <div style={{ fontSize: "1rem", marginTop: "5px" }}>
                    {guessResult.message}
                  </div>
                  {guessResult.points > 0 && (
                    <div
                      style={{
                        fontSize: "1.3rem",
                        fontWeight: "bold",
                        marginTop: "10px",
                        color: "#ffd700",
                      }}
                    >
                      +{guessResult.points} puncte!
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Buton pentru runda următoare */}
          <div style={{ marginTop: "20px", textAlign: "center" }}>
            <button
              onClick={handleReadyForNextRound}
              className="btn btn-success"
              style={{
                padding: "12px 30px",
                fontSize: "1rem",
                fontWeight: "bold",
                borderRadius: "8px",
                width: "auto",
                minWidth: "200px",
              }}
            >
              {isReadyForNextRound ? "Gata" : "Runda Următoare"} (
              {getReadyForNextRoundCount().ready}/
              {getReadyForNextRoundCount().total})
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default VotingSection;
