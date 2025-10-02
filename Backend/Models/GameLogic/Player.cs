using System.ComponentModel.DataAnnotations;
using Backend.Areas.Identity.Data;

namespace Backend.Models
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        public bool IsImpostor { get; set; } = false;

        public bool IsReady { get; set; } = false;

        public int Score { get; set; } = 0;

        public bool IsEliminated { get; set; } = false;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LeftAt { get; set; }

        // Navigation properties
        public GameObject? Game { get; set; }
        public ApplicationUser? User { get; set; }
        public List<Answer> Answers { get; set; } = new();
        public List<Vote> Votes { get; set; } = new();
        public List<Vote> VotesReceived { get; set; } = new();
    }
}
