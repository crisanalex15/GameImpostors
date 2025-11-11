import React, { useState } from "react";
import {
  RoundResponse,
  GameType,
  RoundState,
  PlayerResponse,
} from "../types/game";
import { gameApi } from "../services/api";

interface RoundDisplayProps {
  round: RoundResponse;
  gameType: GameType;
  gameId: string;
  hostId: string;
  currentUserId?: string;
  currentRoundNumber: number;
  maxRounds: number;
  players: PlayerResponse[];
  onStateUpdate: () => void;
  onNewGame?: () => void;
  onLeaveLobby?: () => void;
  onNextRound?: () => void;
}

const RoundDisplay: React.FC<RoundDisplayProps> = ({
  round,
  gameType,
  players,
  hostId,
  currentUserId,
  currentRoundNumber,
  maxRounds,
  onStateUpdate,
  onNewGame,
  onLeaveLobby,
  onNextRound,
}) => {
  const isHost = currentUserId === hostId;
  const isLastRound = currentRoundNumber >= maxRounds;
  const [answer, setAnswer] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmitAnswer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSubmitting) return;

    setIsSubmitting(true);
    try {
      // Pentru WordHidden, trimitem un răspuns gol sau "ready"
      const answerToSubmit = answer.trim() || "ready";
      await gameApi.submitAnswer(round.id, { answer: answerToSubmit });
      setAnswer("");
      onStateUpdate();
    } catch (err: any) {
      alert(err.response?.data?.message || "Eroare la trimiterea răspunsului");
    } finally {
      setIsSubmitting(false);
    }
  };

  const getReadyCount = () => {
    if (gameType !== GameType.WordHidden || round.state !== RoundState.Active) {
      return null;
    }

    // Numără câți jucători au răspuns (sunt gata de vot)
    const readyCount = round.answers ? round.answers.length : 0;
    const totalPlayers = players.length;
    const remainingCount = totalPlayers - readyCount;

    return {
      ready: readyCount,
      total: totalPlayers,
      remaining: remainingCount,
    };
  };

  const getRoundTitle = () => {
    switch (round.state) {
      case RoundState.Waiting:
        return "🎯 Runda începe...";
      case RoundState.Active:
        return gameType === GameType.WordHidden
          ? "💬 Discutați și găsiți impostorul!"
          : "❓ Răspunde la întrebare!";
      case RoundState.Voting:
        return "🗳️ Votează cine crezi că e impostorul!";
      case RoundState.Ended:
        return "✅ Runda s-a terminat";
      default:
        return "🎮 Runda";
    }
  };

  const getRoundDescription = () => {
    switch (round.state) {
      case RoundState.Waiting:
        return "Pregătiți-vă pentru runda următoare...";
      case RoundState.Active:
        return gameType === GameType.WordHidden
          ? "Discutați în camera de chat și încercați să găsiți impostorul. Când sunteți gata, apăsați 'Sunt gata de vot'!"
          : "Răspunde la întrebarea de mai jos. Ai timp limitat!";
      case RoundState.Voting:
        return "Toți jucătorii au răspuns. Acum votați cine credeți că este impostorul!";
      case RoundState.Ended:
        return "Rezultatele rundei au fost calculate.";
      default:
        return "";
    }
  };

  const getContent = () => {
    if (gameType === GameType.Questions) {
      return round.questionText;
    } else {
      return round.word;
    }
  };

  const getContentLabel = () => {
    if (gameType === GameType.Questions) {
      return "Întrebare:";
    } else {
      return "Cuvântul:";
    }
  };

  // Nu afișa nimic dacă este în faza de votare
  if (round.state === RoundState.Voting) {
    return null;
  }

  return (
    round.state === RoundState.Active && (
      <>
    <div className="card">
      <div className="game-status">
        <h2>{getRoundTitle()}</h2>
            <p
              style={{
                textAlign: "center",
                margin: "10px 0",
                color: "#666",
                fontSize: "1rem",
              }}
            >
              {getRoundDescription()}
            </p>
      </div>
          {/* Content Display */}
          {round.state === RoundState.Active && getContent() && (
            <>
      <h3 style={{ textAlign: "center", marginBottom: "20px" }}>
        {getContentLabel()}
      </h3>
      <div
        style={{
          background: "rgba(255, 255, 255, 0.9)",
          padding: "20px",
          borderRadius: "10px",
          textAlign: "center",
          fontSize: "1.2rem",
          fontWeight: "bold",
          marginBottom: "20px",
          border: "2px solid #667eea",
        }}
      >
        {getContent()}
      </div>
            </>
          )}

          {/* Ready to Vote Button for WordHidden */}
          {round.state === RoundState.Active &&
            gameType === GameType.WordHidden &&
            !round.hasPlayerAnswered && (
              <div style={{ textAlign: "center", marginBottom: "20px" }}>
                <button
                  className="btn btn-success"
                  onClick={handleSubmitAnswer}
                  disabled={isSubmitting}
                  style={{
                    fontSize: "1.1rem",
                    padding: "12px 30px",
                    borderRadius: "25px",
                    fontWeight: "bold",
                    background: "linear-gradient(45deg, #28a745, #20c997)",
                    border: "none",
                    color: "white",
                    cursor: "pointer",
                    transition: "all 0.3s ease",
                    boxShadow: "0 4px 15px rgba(40, 167, 69, 0.3)",
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = "scale(1.01)";
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = "scale(1)";
                  }}
                >
                  {isSubmitting
                    ? "Se procesează..."
                    : "Sunt gata de vot!" +
                      " " +
                      getReadyCount()?.ready +
                      "/" +
                      getReadyCount()?.total}
                </button>
              </div>
            )}
      {/* Game Content
      {round.state === RoundState.Active && getContent() && (
        <div className="answer-section">
          <h3 style={{ textAlign: "center", marginBottom: "20px" }}>
            {getContentLabel()}
          </h3>
          <div
            style={{
              background: "rgba(255, 255, 255, 0.9)",
              padding: "20px",
              borderRadius: "10px",
              textAlign: "center",
              fontSize: "1.2rem",
              fontWeight: "bold",
              marginBottom: "20px",
              border: "2px solid #667eea",
            }}
          >
            {getContent()}
          </div>
          Answer Form
          {!round.hasPlayerAnswered && (
            <form onSubmit={handleSubmitAnswer}>
              <div className="form-group">
                <label className="form-label">
                  {gameType === GameType.Questions
                    ? "Răspunde la întrebare:"
                    : "Descrie cuvântul (fără să-l spui direct):"}
                </label>
                <textarea
                  className="form-input"
                  value={answer}
                  onChange={(e) => setAnswer(e.target.value)}
                  placeholder={
                    gameType === GameType.Questions
                      ? "Scrie răspunsul tău aici..."
                      : "Descrie cuvântul fără să-l spui direct..."
                  }
                  rows={4}
                  style={{ resize: "vertical" }}
                  required
                />
              </div>

              <button
                type="submit"
                className="btn btn-success"
                style={{ width: "100%" }}
                disabled={isSubmitting || !answer.trim()}
              >
                {isSubmitting ? "Se trimite..." : "Trimite Răspunsul"}
              </button>
            </form>
          )}
          {round.hasPlayerAnswered && (
            <div
              style={{
                textAlign: "center",
                padding: "20px",
                background: "rgba(40, 167, 69, 0.1)",
                borderRadius: "8px",
                border: "1px solid rgba(40, 167, 69, 0.3)",
                color: "#28a745",
                fontWeight: "bold",
              }}
            >
              ✅ Ai trimis deja răspunsul! Așteaptă ca ceilalți să termine.
            </div>
          )}
        </div>
      )} */}
      {/* Answers Display */}
          {/* {round.state === RoundState.Voting && round.answers.length > 0 && (
        <div className="answer-section">
          <h3 style={{ textAlign: "center", marginBottom: "20px" }}>
            Răspunsurile jucătorilor:
          </h3>
          <div className="grid grid-2">
            {round.answers.map((answer) => (
              <div
                key={answer.id}
                style={{
                  background: "rgba(255, 255, 255, 0.9)",
                  padding: "15px",
                  borderRadius: "8px",
                  border: "1px solid #e1e5e9",
                  marginBottom: "10px",
                }}
              >
                <p style={{ fontWeight: "bold", marginBottom: "5px" }}>
                  Jucător {answer.playerId.slice(-4)}:
                </p>
                <p style={{ margin: 0 }}>{answer.value}</p>
              </div>
            ))}
          </div>
        </div>
      )} */}
          {/* Round Ended - Scoreboard and Options - ONLY shown when game is ended, not just round */}
          {false && round.state === RoundState.Ended && (
        <div
          style={{
            textAlign: "center",
            padding: "20px",
                background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
                color: "white",
                borderRadius: "15px",
                border: "1px solid rgba(255, 255, 255, 0.2)",
              }}
            >
              <h3 style={{ marginBottom: "20px", fontSize: "1.5rem" }}>
                🏆 Scoreboard
              </h3>

              {/* Players Score List */}
              <div style={{ marginBottom: "30px" }}>
                {players
                  .sort((a, b) => (b.score || 0) - (a.score || 0))
                  .map((player, index) => (
                    <div
                      key={player.id}
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        padding: "12px 20px",
                        margin: "8px 0",
                        background: "rgba(255, 255, 255, 0.2)",
                        borderRadius: "10px",
                        border: "1px solid rgba(255, 255, 255, 0.3)",
                      }}
                    >
                      <div
                        style={{
                          display: "flex",
                          alignItems: "center",
                          gap: "10px",
                        }}
                      >
                        <div
                          style={{
                            fontSize: "1.2rem",
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
                        <div>
                          <div
                            style={{ fontWeight: "bold", fontSize: "1.1rem" }}
                          >
                            Jucător {player.id.slice(-4)}
                          </div>
                          <div style={{ fontSize: "0.9rem", opacity: 0.8 }}>
                            {player.isImpostor ? "👹 Impostor" : "👤 Crewmate"}
                            {player.isEliminated && " (Eliminat)"}
                          </div>
                        </div>
                      </div>
                      <div
                        style={{
                          fontSize: "1.3rem",
                          fontWeight: "bold",
                          color: "#ffd700",
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
                {/* Show "Next Round" if not last round */}
                {!isLastRound && onNextRound && (
                  <button
                    onClick={onNextRound}
                    style={{
                      padding: "15px 30px",
                      background: "linear-gradient(45deg, #28a745, #20c997)",
                      color: "white",
                      border: "none",
                      borderRadius: "25px",
                      fontSize: "1.1rem",
                      fontWeight: "bold",
                      cursor: "pointer",
                      transition: "all 0.3s ease",
                      boxShadow: "0 4px 15px rgba(40, 167, 69, 0.3)",
                    }}
                    onMouseEnter={(e) => {
                      e.currentTarget.style.transform = "scale(1.05)";
                      e.currentTarget.style.boxShadow =
                        "0 6px 20px rgba(40, 167, 69, 0.4)";
                    }}
                    onMouseLeave={(e) => {
                      e.currentTarget.style.transform = "scale(1)";
                      e.currentTarget.style.boxShadow =
                        "0 4px 15px rgba(40, 167, 69, 0.3)";
                    }}
                  >
                    ➡️ Runda Următoare
                  </button>
                )}

                {/* Show "New Game" (only for host) and "Leave Lobby" ONLY on last round */}
                {isLastRound && (
                  <>
                    {onNewGame && isHost && (
                      <button
                        onClick={onNewGame}
                        style={{
                          padding: "15px 30px",
                          background:
                            "linear-gradient(45deg, #28a745, #20c997)",
                          color: "white",
                          border: "none",
                          borderRadius: "25px",
                          fontSize: "1.1rem",
                          fontWeight: "bold",
                          cursor: "pointer",
                          transition: "all 0.3s ease",
                          boxShadow: "0 4px 15px rgba(40, 167, 69, 0.3)",
                        }}
                        onMouseEnter={(e) => {
                          e.currentTarget.style.transform = "scale(1.05)";
                          e.currentTarget.style.boxShadow =
                            "0 6px 20px rgba(40, 167, 69, 0.4)";
                        }}
                        onMouseLeave={(e) => {
                          e.currentTarget.style.transform = "scale(1)";
                          e.currentTarget.style.boxShadow =
                            "0 4px 15px rgba(40, 167, 69, 0.3)";
                        }}
                      >
                        🎮 Meci Nou
                      </button>
                    )}

                    {onLeaveLobby && (
                      <button
                        onClick={onLeaveLobby}
                        style={{
                          padding: "15px 30px",
                          background:
                            "linear-gradient(45deg, #dc3545, #c82333)",
                          color: "white",
                          border: "none",
                          borderRadius: "25px",
                          fontSize: "1.1rem",
                          fontWeight: "bold",
                          cursor: "pointer",
                          transition: "all 0.3s ease",
                          boxShadow: "0 4px 15px rgba(220, 53, 69, 0.3)",
                        }}
                        onMouseEnter={(e) => {
                          e.currentTarget.style.transform = "scale(1.05)";
                          e.currentTarget.style.boxShadow =
                            "0 6px 20px rgba(220, 53, 69, 0.4)";
                        }}
                        onMouseLeave={(e) => {
                          e.currentTarget.style.transform = "scale(1)";
                          e.currentTarget.style.boxShadow =
                            "0 4px 15px rgba(220, 53, 69, 0.3)";
                        }}
                      >
                        🚪 Părăsește Lobby
                      </button>
                    )}
                  </>
                )}
              </div>
        </div>
      )}
    </div>
      </>
    )
  );
};

export default RoundDisplay;
