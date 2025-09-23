using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Vote
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RoundId { get; set; }

        [Required]
        public Guid VoterId { get; set; }

        [Required]
        public Guid TargetId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Reason { get; set; }

        // Navigation properties
        public Round? Round { get; set; }
        public Player? Voter { get; set; }
        public Player? Target { get; set; }

        // Helper methods
        public bool IsValidVote()
        {
            return VoterId != TargetId; // Can't vote for yourself
        }
    }
}