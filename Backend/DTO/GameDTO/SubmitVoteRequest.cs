using System.ComponentModel.DataAnnotations;

namespace Backend.DTO.GameDTO
{
    public class SubmitVoteRequest
    {
        [Required]
        public Guid TargetPlayerId { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }
    }
}
