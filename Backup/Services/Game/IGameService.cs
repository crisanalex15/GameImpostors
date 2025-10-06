using Backend.Models;
using Backend.Models.Questions;
using static Backend.Models.GameObject;

namespace Backend.Services.Game
{
    public interface IGameService
    {
        // Lobby Management
        Task<(bool Success, GameObject? Game, string Message)> CreateGameAsync(Guid hostId, GameType gameType, int maxPlayers = 6, string userName = "");
        Task<(bool Success, GameObject? Game, string Message)> JoinGameAsync(string lobbyCode, Guid userId, string userName = "");
        Task<(bool Success, string Message)> LeaveGameAsync(Guid gameId, Guid userId);
        Task<(bool Success, string Message)> SetPlayerReadyAsync(Guid gameId, Guid userId, bool isReady);
        Task<(bool Success, string Message)> StartGameAsync(Guid gameId, Guid hostId);
        Task<(bool Success, string Message)> DeleteGameAsync(Guid gameId);
        // Game State
        Task<GameObject?> GetGameAsync(Guid gameId);
        Task<GameObject?> GetGameByLobbyCodeAsync(string lobbyCode);
        Task<List<Player>> GetGamePlayersAsync(Guid gameId);
        Task<Player?> GetPlayerAsync(Guid gameId, Guid userId);

        // Round Management
        Task<(bool Success, Round? Round, string Message)> CreateRoundAsync(Guid gameId);
        Task<Round?> GetCurrentRoundAsync(Guid gameId);
        Task<Round?> GetRoundByIdAsync(Guid roundId);
        Task<(bool Success, string Message)> SubmitAnswerAsync(Guid roundId, Guid playerId, string answer);
        Task<List<Answer>> GetRoundAnswersAsync(Guid roundId);

        // Voting System
        Task<(bool Success, string Message)> StartVotingPhaseAsync(Guid roundId);
        Task<(bool Success, string Message)> SubmitVoteAsync(Guid roundId, Guid voterId, Guid targetId);
        Task<List<Vote>> GetRoundVotesAsync(Guid roundId);
        Task<(Guid? EliminatedPlayerId, bool IsImpostorEliminated)> CalculateVotingResultsAsync(Guid roundId);

        // Game Logic
        Task<(bool Success, string Message)> EndRoundAsync(Guid roundId);
        Task<(bool Success, string Message)> EndGameAsync(Guid gameId);
        Task<bool> CheckGameEndConditionsAsync(Guid gameId);

        // Utility Methods
        string GenerateUniqueLobbyCode();
        Task<bool> IsLobbyCodeUniqueAsync(string code);
    }
}
