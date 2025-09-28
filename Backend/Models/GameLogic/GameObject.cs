using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class GameObject
    {
        public enum GameState
        {
            Lobby,
            Game,
            Ended
        }

        public enum GameType
        {
            WordHidden, // x knows the word, y doesn't
            Questions // x gets a question, y gets a fake question
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HostId { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string LobbyCode { get; set; } = string.Empty;

        [StringLength(6, MinimumLength = 6)]
        public string? LobbyPrivateCode { get; set; }

        [Range(3, 10)]
        public int MaxPlayers { get; set; } = 6;

        public int CurrentPlayers { get; set; } = 0;

        public GameType Type { get; set; } = GameType.WordHidden;

        public GameState State { get; set; } = GameState.Lobby;

        public int RoundNumber { get; set; } = 0; // 0 = lobby, 1-3 = rounds

        public int MaxRounds { get; set; } = 3;

        public int ImpostorCount { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        [Range(30, 300)]
        public int TimerDuration { get; set; } = 120; // in seconds

        public DateTime? TimerStartedAt { get; set; }

        public DateTime? TimerEndedAt { get; set; }

        // Navigation properties
        public List<Player> Players { get; set; } = new();
        public List<Round> Rounds { get; set; } = new();

        // Helper methods
        public bool CanStartGame()
        {
            return State == GameState.Lobby &&
                   CurrentPlayers >= 2 &&
                   Players.All(p => p.IsReady);
        }

        public bool IsGameActive()
        {
            return State == GameState.Game && RoundNumber > 0 && RoundNumber <= MaxRounds;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}