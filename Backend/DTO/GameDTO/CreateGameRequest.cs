using System.ComponentModel.DataAnnotations;
using static Backend.Models.GameObject;

namespace Backend.DTO.GameDTO
{
    public class CreateGameRequest
    {
        [Required]
        public GameType GameType { get; set; }

        [Range(2, 20)] // Minimum 2 players, maximum 20
        public int MaxPlayers { get; set; } = 20;

        [Range(0, 20)] // 0 = random, 1-20 = fixed number
        public int ImpostorCount { get; set; } = 1;

        [Range(30, 300)]
        public int TimerDuration { get; set; } = 120;

        [Range(1, 10)] // Maximum 10 rounds
        public int MaxRounds { get; set; } = 3;
    }
}
