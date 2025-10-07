using System.ComponentModel.DataAnnotations;

namespace Backend.DTO.GameDTO
{
    public class GuessWordRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Word { get; set; } = string.Empty;
    }
}

