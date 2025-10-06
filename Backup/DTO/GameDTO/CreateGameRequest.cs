using System.ComponentModel.DataAnnotations;
using static Backend.Models.GameObject;

namespace Backend.DTO.GameDTO
{
    public class CreateGameRequest
    {
        [Required]
        public GameType GameType { get; set; }

        [Range(3, 10)]
        public int MaxPlayers { get; set; } = 6;

        [Range(1, 3)]
        public int ImpostorCount { get; set; } = 1;

        [Range(30, 300)]
        public int TimerDuration { get; set; } = 120;

        [Range(1, 5)]
        public int MaxRounds { get; set; } = 3;
    }
}
