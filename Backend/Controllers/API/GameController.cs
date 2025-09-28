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
                    request.MaxPlayers
                );

                if (!success || game == null)
                {
                    return BadRequest(new { message });
                }

                // Update game settings
                game.ImpostorCount = request.ImpostorCount;
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
                var (success, game, message) = await _gameService.JoinGameAsync(request.LobbyCode, userId);

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

        [HttpPost("{gameId}/ready")]
        public async Task<IActionResult> SetReady(Guid gameId, [FromBody] bool isReady)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                var userId = Guid.Parse(user.Id);
                var (success, message) = await _gameService.SetPlayerReadyAsync(gameId, userId, isReady);

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

        #endregion

        #region Game State

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

                // Get player ID from round
                var round = await _gameService.GetCurrentRoundAsync(roundId);
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

                // Get player ID from round
                var round = await _gameService.GetCurrentRoundAsync(roundId);
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

        private async Task<GameStateResponse> BuildGameStateResponse(Guid gameId, Guid currentUserId)
        {
            var game = await _gameService.GetGameAsync(gameId);
            if (game == null)
            {
                throw new ArgumentException("Game not found");
            }

            var currentPlayer = await _gameService.GetPlayerAsync(gameId, currentUserId);
            var currentRound = await _gameService.GetCurrentRoundAsync(gameId);

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
                    response.QuestionText = currentPlayer.IsImpostor
                        ? round.Question.FakeQuestionText
                        : round.Question.QuestionText;
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
