using System.ComponentModel.DataAnnotations;
using Backend.Models.Questions;

namespace Backend.Models
{
    public class Round
    {
        public enum RoundState
        {
            Waiting,    // Waiting for players to join
            Active,     // Round is active, players can answer
            Review,     // Review answers before voting (Question Swap only)
            Voting,     // Voting phase
            Ended       // Round ended
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid GameId { get; set; }

        public Guid? QuestionId { get; set; }
        public Guid? WordHiddenId { get; set; }

        public Guid? ImpostorId { get; set; }

        public RoundState State { get; set; } = RoundState.Waiting;

        public int RoundNumber { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        [Range(30, 300)]
        public int TimeLimit { get; set; } = 120; // in seconds

        public DateTime? TimerStartedAt { get; set; }

        public DateTime? TimerEndedAt { get; set; }

        public bool IsImpostorGuessed { get; set; } = false;

        public string? ImpostorGuess { get; set; }

        public bool IsImpostorGuessCorrect { get; set; } = false;

        // Navigation properties
        public GameObject? Game { get; set; }
        public Player? Impostor { get; set; }
        public Question? Question { get; set; }
        public WordHidden? WordHidden { get; set; }
        public List<Answer> Answers { get; set; } = new();
        public List<Vote> Votes { get; set; } = new();

        // Helper methods
        public bool IsTimerExpired()
        {
            if (!TimerStartedAt.HasValue) return false;
            return DateTime.UtcNow >= TimerStartedAt.Value.AddSeconds(TimeLimit);
        }

        public int GetRemainingTime()
        {
            if (!TimerStartedAt.HasValue) return TimeLimit;
            var elapsed = (DateTime.UtcNow - TimerStartedAt.Value).TotalSeconds;
            return Math.Max(0, TimeLimit - (int)elapsed);
        }

        public void StartTimer()
        {
            TimerStartedAt = DateTime.UtcNow;
            State = RoundState.Active;
        }

        public void EndRound()
        {
            EndedAt = DateTime.UtcNow;
            State = RoundState.Ended;
        }
    }
}