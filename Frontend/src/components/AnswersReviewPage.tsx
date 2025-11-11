import React, { useState, useEffect } from "react";
import { RoundResponse, PlayerResponse } from "../types/game";

interface AnswersReviewPageProps {
  round: RoundResponse;
  players: PlayerResponse[];
  currentUserId?: string;
}

const AnswersReviewPage: React.FC<AnswersReviewPageProps> = ({
  round,
  players,
  currentUserId,
}) => {
  const [countdown, setCountdown] = useState(5);

  // Countdown timer - automaticăm countdown-ul
  useEffect(() => {
    if (countdown > 0) {
      const timer = setTimeout(() => setCountdown(countdown - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [countdown]);

  const getPlayerUserName = (playerId: string) => {
    const player = players.find((p) => p.id === playerId);
    if (currentUserId && player && player.userId === currentUserId) {
      return "Tu";
    }
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
            <h2 style={styles.title}>⏳ Pregătește-te pentru votare!</h2>
            <div style={styles.readySection}>
              <div style={styles.countdownBox}>
                <div style={styles.countdownNumber}>{countdown}</div>
                <div style={styles.countdownText}>Votarea începe în...</div>
              </div>
            </div>
          </>
        ) : (
          // After countdown, show question, answers, and "Se trece la votare..."
          <>
            <h2 style={styles.title}>📝 Răspunsurile tuturor jucătorilor</h2>
            <p style={styles.subtitle}>
              Citește cu atenție răspunsurile și pregătește-te să votezi!
            </p>

            {/* Show the question */}
            <div style={styles.questionBox}>
              <h3 style={styles.questionTitle}>Întrebarea corectă:</h3>
              <p style={styles.questionText}>{round.questionText}</p>
            </div>

            {/* Show all answers */}
            <div style={styles.answersContainer}>
              {round.answers.map((answer) => {
                const player = players.find((p) => p.id === answer.playerId);
                const isCurrentUser = player?.userId === currentUserId;

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
                        {getPlayerIcon(player!)}{" "}
                        {getPlayerUserName(answer.playerId)}
                      </span>
                      {isCurrentUser && <span style={styles.youBadge}>Tu</span>}
                    </div>
                    <div style={styles.answerText}>{answer.value}</div>
                    {answer.isEdited && (
                      <div style={styles.editedLabel}>(editat)</div>
                    )}
                  </div>
                );
              })}
            </div>

            {/* Show "Se trece la votare..." message */}
            <div style={styles.readySection}>
              <div style={styles.transitionMessage}>
                🗳️ Se trece la votare...
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

const styles: { [key: string]: React.CSSProperties } = {
  container: {
    padding: "20px",
    maxWidth: "900px",
    margin: "0 auto",
  },
  card: {
    background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
    borderRadius: "20px",
    padding: "40px",
    boxShadow: "0 10px 40px rgba(0, 0, 0, 0.3)",
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
    border: "3px solid #ffc107",
    background: "rgba(255, 193, 7, 0.1)",
    boxShadow: "0 4px 15px rgba(255, 193, 7, 0.3)",
  },
  answerHeader: {
    display: "flex",
    justifyContent: "space-between",
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
    boxShadow: "0 8px 30px rgba(0, 0, 0, 0.3)",
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
    boxShadow: "0 4px 20px rgba(0, 0, 0, 0.2)",
  },
};

export default AnswersReviewPage;
