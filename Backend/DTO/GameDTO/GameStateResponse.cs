using Backend.Models;
using static Backend.Models.GameObject;

namespace Backend.DTO.GameDTO
{
    public class GameStateResponse
    {
        public Guid Id { get; set; }
        public string LobbyCode { get; set; } = string.Empty;
        public Guid HostId { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public GameType Type { get; set; }
        public GameObject.GameState State { get; set; }
        public int RoundNumber { get; set; }
        public int MaxRounds { get; set; }
        public int TimerDuration { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public List<PlayerResponse> Players { get; set; } = new();
        public RoundResponse? CurrentRound { get; set; }
    }

    public class PlayerResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
        public bool IsImpostor { get; set; }
        public bool IsReady { get; set; }
        public bool IsEliminated { get; set; }
        public int Score { get; set; }
        public DateTime JoinedAt { get; set; }

        // Hide impostor status from other players (will be filtered in controller)
        public bool ShowImpostorStatus { get; set; } = false;
    }

    public class RoundResponse
    {
        public Guid Id { get; set; }
        public int RoundNumber { get; set; }
        public Round.RoundState State { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int TimeLimit { get; set; }
        public int RemainingTime { get; set; }

        // Question/Word data (filtered based on player role)
        public string? QuestionText { get; set; }
        public string? Word { get; set; }

        public List<AnswerResponse> Answers { get; set; } = new();
        public List<VoteResponse> Votes { get; set; } = new();
        public bool HasPlayerAnswered { get; set; }
        public bool HasPlayerVoted { get; set; }
    }

    public class AnswerResponse
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public string Value { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }
    }

    public class VoteResponse
    {
        public Guid Id { get; set; }
        public Guid VoterId { get; set; }
        public Guid TargetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Reason { get; set; }
    }
}
