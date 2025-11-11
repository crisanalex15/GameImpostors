using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend.Areas.Identity.Data;
using Backend.Services.Game;
using Backend.DTO.GameDTO;
using Backend.Models;

namespace Backend.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer,Identity.Application")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GameController(IGameService gameService, UserManager<ApplicationUser> userManager)
        {
            _gameService = gameService;
            _userManager = userManager;
        }

        #region Lobby Management

        // Create a new game
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var (success, game, message) = await _gameService.CreateGameAsync(
                    userId,
                    request.GameType,
                    request.MaxPlayers,
                    user.UserName ?? ""
                );

                if (!success || game == null)
                {
                    return BadRequest(new { message });
                }

                // Update game settings
                if (request.ImpostorCount != 0 && request.ImpostorCount == 4)
                {
                    game.ImpostorCount = game.GetRandomImpostorCount(game.MaxPlayers);
                }
                else
                {
                    game.ImpostorCount = request.ImpostorCount;
                }
                game.TimerDuration = request.TimerDuration;
                game.MaxRounds = request.MaxRounds;

                var response = await BuildGameStateResponse(game.Id, userId);
                return Ok(new { message, game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Join a game
        [HttpPost("join")]
        public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var (success, game, message) = await _gameService.JoinGameAsync(request.LobbyCode, userId, user.UserName ?? "");

                if (!success || game == null)
                {
                    return BadRequest(new { message });
                }

                var response = await BuildGameStateResponse(game.Id, userId);
                return Ok(new { message, game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Leave a game
        [HttpPost("{gameId}/leave")]
        public async Task<IActionResult> LeaveGame(Guid gameId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);

                var (success, message) = await _gameService.LeaveGameAsync(gameId, userId);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Set a player ready
        [HttpPost("{gameId}/ready")]
        public async Task<IActionResult> SetReady(Guid gameId, [FromBody] SetReadyRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var (success, message) = await _gameService.SetPlayerReadyAsync(gameId, userId, request.IsReady);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                var response = await BuildGameStateResponse(gameId, userId);
                return Ok(new { message, game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Start a game
        [HttpPost("{gameId}/start")]
        public async Task<IActionResult> StartGame(Guid gameId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var (success, message) = await _gameService.StartGameAsync(gameId, userId);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                var response = await BuildGameStateResponse(gameId, userId);
                return Ok(new { message, game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Start next round
        [HttpPost("{gameId}/next-round")]
        public async Task<IActionResult> NextRound(Guid gameId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var game = await _gameService.GetGameAsync(gameId);

                if (game == null)
                {
                    return BadRequest(new { message = "Game not found" });
                }

                // Check if all players are ready (no one is eliminated - all continue playing)
                var totalPlayers = game.Players.Count;
                var readyPlayers = game.Players.Where(p => p.IsReady).Count();

                if (readyPlayers < totalPlayers)
                {
                    return BadRequest(new { message = $"Not all players are ready! ({readyPlayers}/{totalPlayers})" });
                }

                // Reset all players' ready status
                foreach (var player in game.Players)
                {
                    player.IsReady = false;
                }

                // Increment round number
                game.RoundNumber++;

                if (game.RoundNumber > game.MaxRounds)
                {
                    await _gameService.EndGameAsync(gameId);
                    var endedResponse = await BuildGameStateResponse(gameId, userId);
                    return Ok(new { message = "Game ended!", game = endedResponse });
                }

                // Create new round
                await _gameService.CreateRoundAsync(gameId);

                var response = await BuildGameStateResponse(gameId, userId);
                return Ok(new { message = $"Round {game.RoundNumber} started!", game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("{gameId}")]
        public async Task<IActionResult> DeleteGame(Guid gameId)
        {
            try
            {
                var (success, message) = await _gameService.DeleteGameAsync(gameId);
                if (!success)
                {
                    return BadRequest(new { message });
                }
                return Ok(new { message = "Game deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        #endregion

        #region Game State

        // Get the state of a game
        [HttpGet("{gameId}")]
        public async Task<IActionResult> GetGameState(Guid gameId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var player = await _gameService.GetPlayerAsync(gameId, userId);

                if (player == null)
                {
                    return BadRequest(new { message = "You are not in this game" });
                }

                var response = await BuildGameStateResponse(gameId, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        #endregion

        #region Round Management

        // Submit an answer
        [HttpPost("round/{roundId}/answer")]
        public async Task<IActionResult> SubmitAnswer(Guid roundId, [FromBody] SubmitAnswerRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);

                // Get round by ID first
                var round = await _gameService.GetRoundByIdAsync(roundId);
                if (round?.Game == null)
                {
                    return BadRequest(new { message = "Round not found" });
                }

                var player = await _gameService.GetPlayerAsync(round.GameId, userId);
                if (player == null)
                {
                    return BadRequest(new { message = "You are not in this game" });
                }

                var (success, message) = await _gameService.SubmitAnswerAsync(roundId, player.Id, request.Answer);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                var response = await BuildGameStateResponse(round.GameId, userId);
                return Ok(new { message, game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        #endregion

        #region Voting System

        // Start voting phase manually (for Question Swap after timer)
        [HttpPost("round/{roundId}/start-voting")]
        public async Task<IActionResult> StartVoting(Guid roundId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var (success, message) = await _gameService.StartVotingPhaseAsync(roundId);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                var round = await _gameService.GetRoundByIdAsync(roundId);
                if (round != null)
                {
                    var userId = Guid.Parse(user.Id);
                    var response = await BuildGameStateResponse(round.GameId, userId);
                    return Ok(new { message, game = response });
                }

                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Submit a vote
        [HttpPost("round/{roundId}/guess-word")]
        public async Task<IActionResult> GuessWord(Guid roundId, [FromBody] GuessWordRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var (success, message, points) = await _gameService.GuessWordAsync(roundId, userId, request.Word);

                if (!success)
                {
                    return BadRequest(new { message, points });
                }

                return Ok(new { message, points });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost("round/{roundId}/vote")]
        public async Task<IActionResult> SubmitVote(Guid roundId, [FromBody] SubmitVoteRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);

                // Get round by ID first
                var round = await _gameService.GetRoundByIdAsync(roundId);
                if (round?.Game == null)
                {
                    return BadRequest(new { message = "Round not found" });
                }

                var player = await _gameService.GetPlayerAsync(round.GameId, userId);
                if (player == null)
                {
                    return BadRequest(new { message = "You are not in this game" });
                }

                var (success, message) = await _gameService.SubmitVoteAsync(roundId, player.Id, request.TargetPlayerId);

                if (!success)
                {
                    return BadRequest(new { message });
                }

                var response = await BuildGameStateResponse(round.GameId, userId);
                return Ok(new { message, game = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        #endregion

        #region Private Helper Methods

        // Build a game state response
        private async Task<GameStateResponse> BuildGameStateResponse(Guid gameId, Guid currentUserId)
        {
            var game = await _gameService.GetGameAsync(gameId);
            if (game == null)
            {
                throw new ArgumentException("Game not found");
            }

            var currentPlayer = await _gameService.GetPlayerAsync(gameId, currentUserId);
            var currentRound = await _gameService.GetCurrentRoundAsync(gameId);

            // Get usernames for all players
            var playerUserNames = new Dictionary<Guid, string>();
            foreach (var player in game.Players)
            {
                var user = await _userManager.FindByIdAsync(player.UserId.ToString());
                if (user != null)
                {
                    playerUserNames[player.UserId] = user.UserName ?? "Unknown";
                }
            }

            var response = new GameStateResponse
            {
                Id = game.Id,
                LobbyCode = game.LobbyCode,
                HostId = game.HostId,
                MaxPlayers = game.MaxPlayers,
                CurrentPlayers = game.CurrentPlayers,
                Type = game.Type,
                State = game.State,
                RoundNumber = game.RoundNumber,
                MaxRounds = game.MaxRounds,
                TimerDuration = game.TimerDuration,
                CreatedAt = game.CreatedAt,
                StartedAt = game.StartedAt,
                Players = game.Players.Select(p => new PlayerResponse
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = playerUserNames.ContainsKey(p.UserId) ? playerUserNames[p.UserId] : "Unknown",
                    IsImpostor = p.IsImpostor,
                    IsReady = p.IsReady,
                    IsEliminated = p.IsEliminated,
                    Score = p.Score,
                    JoinedAt = p.JoinedAt,
                    // Only show impostor status to the player themselves or if game ended
                    ShowImpostorStatus = p.UserId == currentUserId || game.State == GameObject.GameState.Ended
                }).ToList()
            };

            if (currentRound != null)
            {
                response.CurrentRound = await BuildRoundResponse(currentRound, currentPlayer);
            }

            return response;
        }

        // Build a round response
        private async Task<RoundResponse> BuildRoundResponse(Round round, Player? currentPlayer)
        {
            var answers = await _gameService.GetRoundAnswersAsync(round.Id);
            var votes = await _gameService.GetRoundVotesAsync(round.Id);

            var response = new RoundResponse
            {
                Id = round.Id,
                RoundNumber = round.RoundNumber,
                State = round.State,
                StartedAt = round.StartedAt,
                EndedAt = round.EndedAt,
                TimeLimit = round.TimeLimit,
                RemainingTime = round.GetRemainingTime(),
                Answers = answers.Select(a => new AnswerResponse
                {
                    Id = a.Id,
                    PlayerId = a.PlayerId,
                    Value = a.Value,
                    CreatedAt = a.CreatedAt,
                    IsEdited = a.IsEdited
                }).ToList(),
                Votes = votes.Select(v => new VoteResponse
                {
                    Id = v.Id,
                    VoterId = v.VoterId,
                    TargetId = v.TargetId,
                    CreatedAt = v.CreatedAt,
                    Reason = v.Reason
                }).ToList(),
                HasPlayerAnswered = currentPlayer != null && answers.Any(a => a.PlayerId == currentPlayer.Id),
                HasPlayerVoted = currentPlayer != null && votes.Any(v => v.VoterId == currentPlayer.Id)
            };

            // Set question/word text based on player role and game type
            if (currentPlayer != null && round.Game != null)
            {
                if (round.Game.Type == GameObject.GameType.Questions && round.Question != null)
                {
                    // In Review state, everyone sees the CORRECT question (crewmate question)
                    // In Active state, players see question based on their role
                    if (round.State == Round.RoundState.Review || round.State == Round.RoundState.Voting || round.State == Round.RoundState.Ended)
                    {
                        response.QuestionText = round.Question.QuestionText;
                    }
                    else
                    {
                        response.QuestionText = currentPlayer.IsImpostor
                            ? round.Question.FakeQuestionText
                            : round.Question.QuestionText;
                    }
                }
                else if (round.Game.Type == GameObject.GameType.WordHidden && round.WordHidden != null)
                {
                    response.Word = currentPlayer.IsImpostor
                        ? round.WordHidden.FakeWord
                        : round.WordHidden.Word;
                }
            }

            return response;
        }

        #endregion
    }
}
