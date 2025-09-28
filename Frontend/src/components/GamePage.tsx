import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { gameApi } from "../services/api";
import { GameStateResponse, GameState, RoundState } from "../types/game";
import PlayerList from "./PlayerList";
import RoundDisplay from "./RoundDisplay";
import VotingSection from "./VotingSection";
import { useAuth } from "../contexts/AuthContext";

const GamePage: React.FC = () => {
  const { gameId } = useParams<{ gameId: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [gameState, setGameState] = useState<GameStateResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [isReady, setIsReady] = useState(false);

  useEffect(() => {
    if (gameId) {
      loadGameState();
      // Poll for game state updates every 2 seconds
      const interval = setInterval(loadGameState, 2000);
      return () => clearInterval(interval);
    }
  }, [gameId]);

  const loadGameState = async () => {
    if (!gameId) return;

    try {
      const state = await gameApi.getGameState(gameId);
      setGameState(state);
      setIsLoading(false);
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la încărcarea jocului");
      setIsLoading(false);
    }
  };

  const handleSetReady = async () => {
    if (!gameId) return;

    try {
      const newReadyState = !isReady;
      await gameApi.setReady(gameId, newReadyState);
      setIsReady(newReadyState);
      await loadGameState();
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la setarea statusului");
    }
  };

  const handleStartGame = async () => {
    if (!gameId) return;

    try {
      await gameApi.startGame(gameId);
      await loadGameState();
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la pornirea jocului");
    }
  };

  const handleLeaveGame = async () => {
    if (!gameId) return;

    try {
      await gameApi.leaveGame(gameId);
      navigate("/lobby");
    } catch (err: any) {
      setError(err.response?.data?.message || "Eroare la părăsirea jocului");
    }
  };

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Se încarcă jocul...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container">
        <div className="card">
          <div style={{ textAlign: "center", color: "#dc3545" }}>
            <h2>Eroare</h2>
            <p>{error}</p>
            <button
              className="btn btn-primary"
              onClick={() => navigate("/lobby")}
            >
              Înapoi la Lobby
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!gameState) {
    return (
      <div className="container">
        <div className="card">
          <div style={{ textAlign: "center" }}>
            <h2>Jocul nu a fost găsit</h2>
            <button
              className="btn btn-primary"
              onClick={() => navigate("/lobby")}
            >
              Înapoi la Lobby
            </button>
          </div>
        </div>
      </div>
    );
  }

  const isHost = gameState.hostId === user?.id;
  const canStart =
    gameState.state === GameState.Lobby &&
    gameState.players.length >= 2 &&
    gameState.players.every((p) => p.isReady);

  return (
    <div className="container">
      <div className="card">
        {/* Game Header */}
        <div className="game-status">
          <h1>🎮 GameImpostors</h1>
          <p>
            {gameState.state === GameState.Lobby &&
              "Lobby - Așteaptă să înceapă jocul"}
            {gameState.state === GameState.Game &&
              `Runda ${gameState.roundNumber}/${gameState.maxRounds}`}
            {gameState.state === GameState.Ended && "Jocul s-a terminat"}
          </p>
          <p>
            Cod Lobby: <strong>{gameState.lobbyCode}</strong>
          </p>
        </div>

        {/* Game Controls */}
        <div style={{ textAlign: "center", marginBottom: "30px" }}>
          {gameState.state === GameState.Lobby && (
            <div>
              <button
                className={`btn ${isReady ? "btn-success" : "btn-primary"}`}
                onClick={handleSetReady}
                style={{ marginRight: "10px" }}
              >
                {isReady ? "✓ Gata" : "Nu sunt gata"}
              </button>

              {isHost && (
                <button
                  className="btn btn-success"
                  onClick={handleStartGame}
                  disabled={!canStart}
                >
                  {canStart ? "Începe Jocul" : "Nu toți jucătorii sunt gata"}
                </button>
              )}
            </div>
          )}

          <button
            className="btn btn-danger"
            onClick={handleLeaveGame}
            style={{ marginTop: "10px" }}
          >
            Părăsește Jocul
          </button>
        </div>

        {/* Players List */}
        <PlayerList
          players={gameState.players}
          gameState={gameState.state}
          currentRound={gameState.currentRound}
          currentUserId={user?.id}
        />

        {/* Current Round Display */}
        {gameState.currentRound && (
          <RoundDisplay
            round={gameState.currentRound}
            gameType={gameState.type}
            gameId={gameId!}
            onStateUpdate={loadGameState}
          />
        )}

        {/* Voting Section */}
        {gameState.currentRound?.state === RoundState.Voting && (
          <VotingSection
            round={gameState.currentRound}
            players={gameState.players}
            gameId={gameId!}
            currentUserId={user?.id}
            onVoteSubmitted={loadGameState}
          />
        )}
      </div>
    </div>
  );
};

export default GamePage;
