using System.ComponentModel.DataAnnotations;

namespace Backend.DTO.GameDTO
{
    public class SubmitAnswerRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Answer { get; set; } = string.Empty;
    }
}
