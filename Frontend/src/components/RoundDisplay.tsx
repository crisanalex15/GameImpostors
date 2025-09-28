import React, { useState, useEffect } from "react";
import { RoundResponse, GameType, RoundState } from "../types/game";
import { gameApi } from "../services/api";

interface RoundDisplayProps {
  round: RoundResponse;
  gameType: GameType;
  gameId: string;
  onStateUpdate: () => void;
}

const RoundDisplay: React.FC<RoundDisplayProps> = ({
  round,
  gameType,
  onStateUpdate,
}) => {
  const [answer, setAnswer] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [timeLeft, setTimeLeft] = useState(round.remainingTime);

  useEffect(() => {
    setTimeLeft(round.remainingTime);
  }, [round.remainingTime]);

  useEffect(() => {
    if (round.state === RoundState.Active && timeLeft > 0) {
      const timer = setInterval(() => {
        setTimeLeft((prev) => {
          if (prev <= 1) {
            onStateUpdate(); // Refresh state when time runs out
            return 0;
          }
          return prev - 1;
        });
      }, 1000);

      return () => clearInterval(timer);
    }
  }, [round.state, timeLeft, onStateUpdate]);

  const handleSubmitAnswer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!answer.trim() || isSubmitting) return;

    setIsSubmitting(true);
    try {
      await gameApi.submitAnswer(round.id, { answer: answer.trim() });
      setAnswer("");
      onStateUpdate();
    } catch (err: any) {
      alert(err.response?.data?.message || "Eroare la trimiterea răspunsului");
    } finally {
      setIsSubmitting(false);
    }
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  };

  const getRoundTitle = () => {
    switch (round.state) {
      case RoundState.Waiting:
        return "Așteaptă să înceapă runda...";
      case RoundState.Active:
        return "Runda activă - Răspunde la întrebare!";
      case RoundState.Voting:
        return "Faza de votare - Votează cine crezi că e impostorul!";
      case RoundState.Ended:
        return "Runda s-a terminat";
      default:
        return "Runda";
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

  return (
    <div className="card">
      <div className="game-status">
        <h2>{getRoundTitle()}</h2>
        <p>Runda {round.roundNumber}</p>
      </div>

      {/* Timer */}
      {round.state === RoundState.Active && (
        <div className="timer">
          <div className="timer-value">{formatTime(timeLeft)}</div>
          <p style={{ color: "rgba(255, 255, 255, 0.8)", margin: 0 }}>
            Timp rămas
          </p>
        </div>
      )}

      {/* Game Content */}
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

          {/* Answer Form */}
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
      )}

      {/* Answers Display */}
      {round.state === RoundState.Voting && round.answers.length > 0 && (
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
      )}

      {/* Round Ended */}
      {round.state === RoundState.Ended && (
        <div
          style={{
            textAlign: "center",
            padding: "20px",
            background: "rgba(108, 117, 125, 0.1)",
            borderRadius: "8px",
            border: "1px solid rgba(108, 117, 125, 0.3)",
          }}
        >
          <h3>Runda s-a terminat!</h3>
          <p>Rezultatele vor fi afișate în curând...</p>
        </div>
      )}
    </div>
  );
};

export default RoundDisplay;
