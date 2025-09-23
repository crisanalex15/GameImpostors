using System.ComponentModel.DataAnnotations;

namespace Backend.DTO.GameDTO
{
    public class JoinGameRequest
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string LobbyCode { get; set; } = string.Empty;
    }
}
