import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { gameApi } from "../services/api";

export const useGameRedirect = () => {
  const { currentGameId, setCurrentGameId, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const checkActiveGame = async () => {
      // Verifică doar dacă utilizatorul este autentificat și există un gameId
      if (!isAuthenticated || !currentGameId) return;

      console.log(`Verificare joc activ pentru gameId: ${currentGameId}`);

      try {
        // Verifică dacă jocul încă există și utilizatorul este în el
        const gameState = await gameApi.getGameState(currentGameId);

        // Dacă jocul există și utilizatorul este în el, redirecționează
        if (gameState) {
          console.log(`Jocul există, redirecționare la /game/${currentGameId}`);
          navigate(`/game/${currentGameId}`);
        }
      } catch (error) {
        // Dacă jocul nu mai există sau utilizatorul nu mai este în el
        console.log(
          "Jocul nu mai există sau utilizatorul nu mai este în el",
          error
        );
        setCurrentGameId(null);
      }
    };

    // Execută verificarea cu o mică întârziere pentru a permite context-ului să se actualizeze
    const timeoutId = setTimeout(checkActiveGame, 100);

    return () => clearTimeout(timeoutId);
  }, [currentGameId, isAuthenticated, navigate, setCurrentGameId]);
};
