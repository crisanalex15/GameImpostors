import React, { useState, useEffect } from "react";
import { RoundResponse, PlayerResponse } from "../types/game";
import { gameApi } from "../services/api";

interface AnswersReviewPageProps {
  round: RoundResponse;
  players: PlayerResponse[];
  currentUserId?: string;
  onStateUpdate?: () => void;
}

const AnswersReviewPage: React.FC<AnswersReviewPageProps> = ({
  round,
  players,
  currentUserId,
  onStateUpdate,
}) => {
  // keep prop for callers, even if not used explicitly
  void currentUserId;
  const [countdown, setCountdown] = useState(5);
  const [isStartingVoting, setIsStartingVoting] = useState(false);

  // Countdown timer - automaticăm countdown-ul
  useEffect(() => {
    if (countdown > 0) {
      const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [countdown]);

  const handleStartVoting = async () => {
    if (isStartingVoting) return;

    setIsStartingVoting(true);
    try {
      await gameApi.startVoting(round.id);
      if (onStateUpdate) {
        onStateUpdate();
      }
    } catch (err: any) {
      console.error("Error starting voting:", err);
      alert(err.response?.data?.message || "Eroare la trecerea la votare");
    } finally {
      setIsStartingVoting(false);
    }
  };

  const getPlayerUserName = (playerId: string) => {
    const player = players.find((p) => p.id === playerId);
    return player?.userName || `Jucător ${playerId.slice(-4)}`;
  };

  const getPlayerIcon = (player: PlayerResponse) => {
    if (player.isEliminated) return "💀";
    return "👤";
  };

  return (
    <div style={styles.container}>
      <div style={styles.card}>
        {countdown > 0 ? (
          // Show only countdown during the 5 seconds
          <>
            <h2 style={styles.title}>Pregătește-te pentru votare!</h2>
            <div style={styles.readySection}>
              <div style={styles.countdownBox}>
                <div style={styles.countdownNumber}>{countdown}</div>
                <div style={styles.countdownText}>Votarea începe în...</div>
              </div>
            </div>
          </>
        ) : (
          // After countdown, show question, current player's answer, and button to start voting
          <>
            <h2 style={styles.title}>Răspunsul tău</h2>
            <p style={styles.subtitle}>
              Vezi întrebarea corectă și răspunsul tău, apoi treci la votare!
            </p>

            {/* Show the question */}
            <div style={styles.questionBox}>
              <h3 style={styles.questionTitle}>Întrebarea corectă:</h3>
              <p style={styles.questionText}>{round.questionText}</p>
            </div>

            {/* Show all answers (Question Swap review) */}
            {round.answers && round.answers.length > 0 ? (
              <div style={styles.answersContainer}>
                {round.answers.map((answer) => {
                  const player = players.find((p) => p.id === answer.playerId);
                  const isCurrentUser =
                    currentUserId && player && player.userId === currentUserId;
                  return (
                    <div
                      key={answer.id}
                      style={{
                        ...styles.answerCard,
                        ...(isCurrentUser ? styles.currentUserAnswer : {}),
                      }}
                    >
                      <div style={styles.answerHeader}>
                        <span style={styles.playerName}>
                          {player && getPlayerIcon(player) && (
                            <span style={{ marginRight: "5px" }}>
                              {getPlayerIcon(player)}
                            </span>
                          )}
                          {getPlayerUserName(answer.playerId)}
                        </span>
                      </div>
                      <div style={styles.answerText}>{answer.value}</div>
                      {answer.isEdited && (
                        <div style={styles.editedLabel}>(editat)</div>
                      )}
                    </div>
                  );
                })}
              </div>
            ) : (
              <div style={styles.noAnswerMessage}>Nu există răspunsuri.</div>
            )}

            {/* Button to start voting */}
            <div style={styles.readySection}>
              <button
                onClick={handleStartVoting}
                disabled={isStartingVoting}
                style={{
                  ...styles.voteButton,
                  ...(isStartingVoting ? styles.buttonDisabled : {}),
                }}
              >
                {isStartingVoting ? "Se trece la votare..." : "Treci la votare"}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  container: {
    padding: "0",
    maxWidth: "1000px",
    margin: "0 auto",
    width: "100%",
  },
  card: {
    background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
    borderTopLeftRadius: "0px",
    borderTopRightRadius: "0px",
    borderBottomLeftRadius: "12px",
    borderBottomRightRadius: "12px",
    padding: "24px",
    color: "white",
  },
  title: {
    fontSize: "32px",
    fontWeight: "bold",
    marginBottom: "10px",
    textAlign: "center" as const,
  },
  subtitle: {
    fontSize: "18px",
    textAlign: "center" as const,
    marginBottom: "30px",
    opacity: 0.9,
  },
  questionBox: {
    background: "rgba(255, 255, 255, 0.2)",
    borderRadius: "15px",
    padding: "25px",
    marginBottom: "30px",
    backdropFilter: "blur(10px)",
  },
  questionTitle: {
    fontSize: "20px",
    fontWeight: "bold",
    marginBottom: "15px",
    margin: 0,
  },
  questionText: {
    fontSize: "18px",
    lineHeight: "1.6",
    margin: 0,
  },
  answersContainer: {
    display: "flex",
    flexDirection: "column" as const,
    gap: "20px",
    marginBottom: "30px",
  },
  answerCard: {
    background: "rgba(255, 255, 255, 0.95)",
    borderRadius: "12px",
    padding: "20px",
    border: "2px solid rgba(102, 126, 234, 0.3)",
    transition: "all 0.3s ease",
    color: "#333",
  },
  currentUserAnswer: {
    border: "2px solid #f5576c",
  },
  answerHeader: {
    display: "flex",
    alignItems: "center",
    marginBottom: "12px",
  },
  playerName: {
    fontSize: "18px",
    fontWeight: "bold",
    color: "#667eea",
  },
  youBadge: {
    background: "#ffc107",
    color: "white",
    padding: "4px 12px",
    borderRadius: "20px",
    fontSize: "14px",
    fontWeight: "bold",
  },
  answerText: {
    fontSize: "16px",
    lineHeight: "1.6",
    color: "#333",
  },
  editedLabel: {
    fontSize: "12px",
    color: "#999",
    marginTop: "8px",
    fontStyle: "italic" as const,
  },
  readySection: {
    textAlign: "center" as const,
    marginTop: "40px",
  },
  countdownBox: {
    display: "flex",
    flexDirection: "column" as const,
    alignItems: "center",
    gap: "15px",
  },
  countdownNumber: {
    fontSize: "80px",
    fontWeight: "bold",
    color: "white",
    background: "rgba(255, 255, 255, 0.2)",
    borderRadius: "50%",
    width: "150px",
    height: "150px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    animation: "pulse 1s infinite",
  },
  countdownText: {
    fontSize: "24px",
    fontWeight: "bold",
    color: "rgba(255, 255, 255, 0.95)",
    textShadow: "0 2px 10px rgba(0, 0, 0, 0.3)",
  },
  transitionMessage: {
    fontSize: "32px",
    fontWeight: "bold",
    color: "white",
    padding: "30px",
    background: "rgba(255, 255, 255, 0.2)",
    borderRadius: "15px",
    textAlign: "center" as const,
  },
  voteButton: {
    padding: "12px 24px",
    fontSize: "16px",
    fontWeight: "600",
    borderRadius: "8px",
    border: "none",
    background: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
    color: "white",
    cursor: "pointer",
    transition: "all 0.2s ease",
    minHeight: "44px",
    minWidth: "200px",
  },
  buttonDisabled: {
    opacity: 0.6,
    cursor: "not-allowed",
  },
  noAnswerMessage: {
    fontSize: "18px",
    color: "rgba(255, 255, 255, 0.8)",
    textAlign: "center" as const,
    padding: "20px",
    background: "rgba(255, 255, 255, 0.1)",
    borderRadius: "10px",
    marginBottom: "30px",
  },
};

export default AnswersReviewPage;
