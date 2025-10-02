using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Answer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RoundId { get; set; }

        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string NormalizedValue { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; } = false;

        // Navigation properties
        public Round? Round { get; set; }
        public Player? Player { get; set; }

        // Helper methods
        public void UpdateAnswer(string newValue)
        {
            Value = newValue;
            UpdatedAt = DateTime.UtcNow;
            IsEdited = true;
            NormalizedValue = newValue.ToUpper();
        }
    }
}