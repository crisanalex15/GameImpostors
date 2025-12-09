import React, { useState } from "react";
import { RoundResponse, RoundState } from "../types/game";
import { gameApi } from "../services/api";

interface QuestionSwapDisplayProps {
  round: RoundResponse;
  onAnswerSubmitted: () => void;
}

const QuestionSwapDisplay: React.FC<QuestionSwapDisplayProps> = ({
  round,
  onAnswerSubmitted,
}) => {
  const [answer, setAnswer] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmitAnswer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!answer.trim() || isSubmitting) return;

    setIsSubmitting(true);
    setError("");

    try {
      await gameApi.submitAnswer(round.id, { answer });
      setAnswer("");
      onAnswerSubmitted();
    } catch (err: any) {
      setError(
        err.response?.data?.message || "Eroare la trimiterea răspunsului"
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  // Show answer input (Active state)
  if (round.state === RoundState.Active && !round.hasPlayerAnswered) {
    return (
      <div style={styles.container}>
        <div style={styles.questionCard}>
          <h2 style={styles.title}>Scrie răspunsul tău:</h2>
          <div style={styles.questionBox}>
            <p style={styles.questionText}>{round.questionText}</p>
          </div>

          <form onSubmit={handleSubmitAnswer} style={styles.form}>
            <label style={styles.label}>Răspunsul tău:</label>
            <textarea
              value={answer}
              onChange={(e) => setAnswer(e.target.value)}
              placeholder="Scrie răspunsul tău aici..."
              rows={4}
              style={styles.textarea}
              disabled={isSubmitting}
              required
              autoFocus
            />
            {error && <div style={styles.error}>{error}</div>}
            <button
              type="submit"
              disabled={isSubmitting || !answer.trim()}
              style={{
                ...styles.button,
                ...(isSubmitting || !answer.trim()
                  ? styles.buttonDisabled
                  : {}),
              }}
            >
              {isSubmitting ? "Se trimite..." : "Trimite Răspunsul"}
            </button>
          </form>
        </div>
      </div>
    );
  }

  // Waiting for other players to answer
  if (round.state === RoundState.Active && round.hasPlayerAnswered) {
    return (
      <div style={styles.container}>
        <div style={styles.waitingCard}>
          <h2 style={styles.title}>Așteptăm ceilalți jucători...</h2>
          <p style={styles.waitingText}>
            Ai trimis răspunsul! Așteaptă ca toți jucătorii să răspundă.
          </p>
        </div>
      </div>
    );
  }

  return null;
};

const styles: { [key: string]: React.CSSProperties } = {
  container: {
    padding: "0",
    maxWidth: "1000px",
    margin: "0 auto",
    width: "100%",
  },
  questionCard: {
    background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
    borderTopLeftRadius: "0px",
    borderTopRightRadius: "0px",
    borderBottomLeftRadius: "12px",
    borderBottomRightRadius: "12px",
    padding: "24px",
    color: "white",
  },
  title: {
    fontSize: "28px",
    fontWeight: "bold",
    marginBottom: "20px",
    textAlign: "center" as const,
  },
  questionBox: {
    background: "rgba(255, 255, 255, 0.2)",
    borderRadius: "10px",
    padding: "20px",
    marginBottom: "25px",
    backdropFilter: "blur(10px)",
  },
  questionText: {
    fontSize: "20px",
    lineHeight: "1.6",
    margin: 0,
    textAlign: "center" as const,
  },
  form: {
    display: "flex",
    flexDirection: "column" as const,
    gap: "15px",
  },
  label: {
    fontSize: "18px",
    fontWeight: "600",
  },
  textarea: {
    padding: "15px",
    fontSize: "16px",
    borderRadius: "8px",
    border: "2px solid rgba(255, 255, 255, 0.3)",
    background: "rgba(255, 255, 255, 0.9)",
    color: "#333",
    resize: "vertical" as const,
    minHeight: "100px",
  },
  button: {
    padding: "12px 24px",
    fontSize: "16px",
    fontWeight: "600",
    borderRadius: "8px",
    border: "none",
    background: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
    color: "white",
    cursor: "pointer",
    transition: "all 0.2s ease",
    width: "100%",
    minHeight: "44px",
  },
  buttonDisabled: {
    opacity: 0.5,
    cursor: "not-allowed",
  },
  error: {
    color: "#ff6b6b",
    background: "rgba(255, 255, 255, 0.9)",
    padding: "10px",
    borderRadius: "5px",
    fontSize: "14px",
  },
  waitingCard: {
    background: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
    borderTopLeftRadius: "0px",
    borderTopRightRadius: "0px",
    borderBottomLeftRadius: "12px",
    borderBottomRightRadius: "12px",
    padding: "40px",
    textAlign: "center" as const,
    color: "white",
  },
  waitingText: {
    fontSize: "18px",
    marginTop: "15px",
  },
};

export default QuestionSwapDisplay;
