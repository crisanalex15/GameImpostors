using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Questions
{
    public class WordHidden
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Word { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FakeWord { get; set; } = "Impostor";

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Category { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Difficulty { get; set; } = 1; // 1-5 scale (1 = easy, 5 = very hard)

        [Range(1, 5)]
        public int PlayerReview { get; set; } = 0; // 0 = not rated, 1-5 = rating

        public int ReviewCount { get; set; } = 0; // How many players rated this word

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public List<Round> Rounds { get; set; } = new();

        // Helper methods
        public void UpdateWord(string word, string fakeWord, string category)
        {
            Word = word;
            FakeWord = fakeWord;
            Category = category;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddReview(int rating)
        {
            if (rating < 1 || rating > 5) return;

            if (ReviewCount == 0)
            {
                PlayerReview = rating;
                ReviewCount = 1;
            }
            else
            {
                // Calculate new average rating
                int totalRating = PlayerReview * ReviewCount + rating;
                ReviewCount++;
                PlayerReview = totalRating / ReviewCount;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public double GetAverageRating()
        {
            return ReviewCount > 0 ? (double)PlayerReview : 0;
        }

        public bool IsRated()
        {
            return ReviewCount > 0;
        }
    }
}
