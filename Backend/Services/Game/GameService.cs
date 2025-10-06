using Backend.Areas.Identity.Data;
using Backend.Models;
using Backend.Models.Questions;
using Microsoft.EntityFrameworkCore;
using static Backend.Models.GameObject;

namespace Backend.Services.Game
{
    public class GameService : IGameService
    {
        private readonly AuthDbContext _context;
        private readonly Random _random;

        public GameService(AuthDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        #region Lobby Management

        public async Task<(bool Success, GameObject? Game, string Message)> CreateGameAsync(Guid hostId, GameType gameType, int maxPlayers = 6, string userName = "")
        {
            try
            {
                var lobbyCode = GenerateUniqueLobbyCode();
                while (!await IsLobbyCodeUniqueAsync(lobbyCode))
                {
                    lobbyCode = GenerateUniqueLobbyCode();
                }

                var game = new GameObject
                {
                    HostId = hostId,
                    LobbyCode = lobbyCode,
                    MaxPlayers = maxPlayers,
                    CurrentPlayers = 1,
                    Type = gameType,
                    State = GameObject.GameState.Lobby,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Games.Add(game);

                // Add host as first player
                var hostPlayer = new Player
                {
                    UserId = hostId,
                    GameId = game.Id,
                    IsReady = false,
                    JoinedAt = DateTime.UtcNow,
                    UserName = userName
                };

                _context.Players.Add(hostPlayer);
                await _context.SaveChangesAsync();

                return (true, game, "Game created successfully!");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error creating game: {ex.Message}");
            }
        }

        public async Task<(bool Success, GameObject? Game, string Message)> JoinGameAsync(string lobbyCode, Guid userId, string userName = "")
        {
            try
            {
                var game = await GetGameByLobbyCodeAsync(lobbyCode);
                if (game == null)
                {
                    return (false, null, "Game not found!");
                }

                if (game.State != GameObject.GameState.Lobby)
                {
                    return (false, null, "Game has already started!");
                }

                if (game.CurrentPlayers >= game.MaxPlayers)
                {
                    return (false, null, "Game is full!");
                }

                // Check if player is already in the game
                var existingPlayer = await _context.Players
                    .FirstOrDefaultAsync(p => p.GameId == game.Id && p.UserId == userId);

                if (existingPlayer != null)
                {
                    return (false, null, "You are already in this game!");
                }

                var player = new Player
                {
                    UserId = userId,
                    GameId = game.Id,
                    IsReady = false,
                    JoinedAt = DateTime.UtcNow,
                    UserName = userName
                };

                _context.Players.Add(player);
                game.CurrentPlayers++;
                game.UpdateTimestamp();

                await _context.SaveChangesAsync();

                return (true, game, "Joined game successfully!");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error joining game: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteGameAsync(Guid gameId)
        {
            try
            {
                var game = await GetGameAsync(gameId);
                if (game == null)
                {
                    return (false, "Game not found!");
                }
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
                return (true, "Game deleted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting game: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> LeaveGameAsync(Guid gameId, Guid userId)
        {
            try
            {
                var game = await GetGameAsync(gameId);
                if (game == null)
                {
                    return (false, "Game not found!");
                }

                var player = await GetPlayerAsync(gameId, userId);
                if (player == null)
                {
                    return (false, "You are not in this game!");
                }

                _context.Players.Remove(player);
                game.CurrentPlayers--;

                // If host leaves, assign new host or end game
                if (game.HostId == userId)
                {
                    var remainingPlayers = await _context.Players
                        .Where(p => p.GameId == gameId && p.UserId != userId)
                        .ToListAsync();

                    if (remainingPlayers.Any())
                    {
                        game.HostId = remainingPlayers.First().UserId;
                    }
                    else
                    {
                        // No players left, end game
                        game.State = GameObject.GameState.Ended;
                        game.EndedAt = DateTime.UtcNow;
                    }
                }

                game.UpdateTimestamp();
                await _context.SaveChangesAsync();

                return (true, "Left game successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error leaving game: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SetPlayerReadyAsync(Guid gameId, Guid userId, bool isReady)
        {
            try
            {
                var player = await GetPlayerAsync(gameId, userId);
                if (player == null)
                {
                    return (false, "Player not found in this game!");
                }

                var game = await GetGameAsync(gameId);
                if (game == null || game.State != GameObject.GameState.Lobby)
                {
                    return (false, "Cannot change ready status - game is not in lobby!");
                }

                player.IsReady = isReady;
                game.UpdateTimestamp();
                await _context.SaveChangesAsync();

                return (true, $"Player ready status set to {isReady}!");
            }
            catch (Exception ex)
            {
                return (false, $"Error setting ready status: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> StartGameAsync(Guid gameId, Guid hostId)
        {
            try
            {
                var game = await GetGameAsync(gameId);
                if (game == null)
                {
                    return (false, "Game not found!");
                }

                if (game.HostId != hostId)
                {
                    return (false, "Only the host can start the game!");
                }

                if (game.State != GameObject.GameState.Lobby)
                {
                    return (false, "Game has already started!");
                }

                if (!game.CanStartGame())
                {
                    return (false, "Cannot start game - not enough players or not all players are ready!");
                }

                // Assign impostor roles
                await AssignImpostorRolesAsync(gameId);

                game.State = GameObject.GameState.Game;
                game.StartedAt = DateTime.UtcNow;
                game.RoundNumber = 1;
                game.UpdateTimestamp();

                await _context.SaveChangesAsync();

                // Create first round
                var (success, round, message) = await CreateRoundAsync(gameId);
                if (!success)
                {
                    return (false, $"Game started but failed to create first round: {message}");
                }

                return (true, "Game started successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error starting game: {ex.Message}");
            }
        }

        #endregion

        #region Game State

        public async Task<GameObject?> GetGameAsync(Guid gameId)
        {
            return await _context.Games
                .Include(g => g.Players)
                .Include(g => g.Rounds)
                .FirstOrDefaultAsync(g => g.Id == gameId);
        }

        public async Task<GameObject?> GetGameByLobbyCodeAsync(string lobbyCode)
        {
            return await _context.Games
                .Include(g => g.Players)
                .Include(g => g.Rounds)
                .FirstOrDefaultAsync(g => g.LobbyCode == lobbyCode);
        }

        public async Task<List<Player>> GetGamePlayersAsync(Guid gameId)
        {
            return await _context.Players
                .Where(p => p.GameId == gameId)
                .ToListAsync();
        }

        public async Task<Player?> GetPlayerAsync(Guid gameId, Guid userId)
        {
            return await _context.Players
                .FirstOrDefaultAsync(p => p.GameId == gameId && p.UserId == userId);
        }

        #endregion

        #region Round Management

        public async Task<(bool Success, Round? Round, string Message)> CreateRoundAsync(Guid gameId)
        {
            try
            {
                var game = await GetGameAsync(gameId);
                if (game == null)
                {
                    return (false, null, "Game not found!");
                }

                if (game.State != GameObject.GameState.Game)
                {
                    return (false, null, "Game is not active!");
                }

                // Get impostor for this round
                var impostor = game.Players.FirstOrDefault(p => p.IsImpostor);
                if (impostor == null)
                {
                    return (false, null, "No impostor found!");
                }

                var round = new Round
                {
                    GameId = gameId,
                    ImpostorId = impostor.Id,
                    RoundNumber = game.RoundNumber,
                    State = Round.RoundState.Active,
                    TimeLimit = 0, // No timer
                    StartedAt = DateTime.UtcNow
                };

                // Assign question or word based on game type
                if (game.Type == GameObject.GameType.Questions)
                {
                    var question = await GetRandomQuestionAsync();
                    if (question != null)
                    {
                        round.QuestionId = question.Id;
                    }
                }
                else if (game.Type == GameObject.GameType.WordHidden)
                {
                    var word = await GetRandomWordAsync();
                    if (word != null)
                    {
                        round.WordHiddenId = word.Id;
                    }
                }

                // No timer - players control the pace
                _context.Rounds.Add(round);
                await _context.SaveChangesAsync();

                return (true, round, "Round created successfully!");
            }
            catch (Exception ex)
            {
                return (false, null, $"Error creating round: {ex.Message}");
            }
        }

        public async Task<Round?> GetCurrentRoundAsync(Guid gameId)
        {
            return await _context.Rounds
                .Include(r => r.Game)
                .Include(r => r.Question)
                .Include(r => r.WordHidden)
                .Include(r => r.Answers)
                .Include(r => r.Votes)
                .Where(r => r.GameId == gameId)
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<Round?> GetRoundByIdAsync(Guid roundId)
        {
            return await _context.Rounds
                .Include(r => r.Game)
                .Include(r => r.Question)
                .Include(r => r.WordHidden)
                .Include(r => r.Answers)
                .Include(r => r.Votes)
                .FirstOrDefaultAsync(r => r.Id == roundId);
        }

        public async Task<(bool Success, string Message)> SubmitAnswerAsync(Guid roundId, Guid playerId, string answer)
        {
            try
            {
                var round = await _context.Rounds
                    .Include(r => r.Game)
                    .FirstOrDefaultAsync(r => r.Id == roundId);

                if (round == null)
                {
                    return (false, "Round not found!");
                }

                if (round.State != Round.RoundState.Active)
                {
                    return (false, "Round is not active!");
                }

                if (round.IsTimerExpired())
                {
                    return (false, "Time is up!");
                }

                // Check if player already answered
                var existingAnswer = await _context.Answers
                    .FirstOrDefaultAsync(a => a.RoundId == roundId && a.PlayerId == playerId);

                if (existingAnswer != null)
                {
                    // Update existing answer
                    existingAnswer.UpdateAnswer(answer);
                }
                else
                {
                    // Create new answer
                    var newAnswer = new Answer
                    {
                        RoundId = roundId,
                        PlayerId = playerId,
                        Value = answer,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Answers.Add(newAnswer);
                }

                await _context.SaveChangesAsync();

                // Check if all players have answered
                var game = await GetGameAsync(round.GameId);
                if (game != null)
                {
                    var activePlayers = game.Players.Where(p => !p.IsEliminated).Count();
                    var answersCount = await _context.Answers.CountAsync(a => a.RoundId == roundId);

                    if (answersCount >= activePlayers)
                    {
                        // Move to voting phase
                        await StartVotingPhaseAsync(roundId);
                    }
                }

                return (true, "Answer submitted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error submitting answer: {ex.Message}");
            }
        }

        public async Task<List<Answer>> GetRoundAnswersAsync(Guid roundId)
        {
            return await _context.Answers
                .Include(a => a.Player)
                .Where(a => a.RoundId == roundId)
                .ToListAsync();
        }

        #endregion

        #region Voting System

        public async Task<(bool Success, string Message)> StartVotingPhaseAsync(Guid roundId)
        {
            try
            {
                var round = await _context.Rounds.FindAsync(roundId);
                if (round == null)
                {
                    return (false, "Round not found!");
                }

                round.State = Round.RoundState.Voting;
                await _context.SaveChangesAsync();

                return (true, "Voting phase started!");
            }
            catch (Exception ex)
            {
                return (false, $"Error starting voting phase: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> SubmitVoteAsync(Guid roundId, Guid voterId, Guid targetId)
        {
            try
            {
                var round = await _context.Rounds.FindAsync(roundId);
                if (round == null)
                {
                    return (false, "Round not found!");
                }

                if (round.State != Round.RoundState.Voting)
                {
                    return (false, "Voting is not active!");
                }

                if (voterId == targetId)
                {
                    return (false, "You cannot vote for yourself!");
                }

                // Check if voter already voted
                var existingVote = await _context.Votes
                    .FirstOrDefaultAsync(v => v.RoundId == roundId && v.VoterId == voterId);

                if (existingVote != null)
                {
                    // Update existing vote
                    existingVote.TargetId = targetId;
                }
                else
                {
                    // Create new vote
                    var vote = new Vote
                    {
                        RoundId = roundId,
                        VoterId = voterId,
                        TargetId = targetId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Votes.Add(vote);
                }

                await _context.SaveChangesAsync();

                // Check if majority has voted (more than half of active players)
                var game = await GetGameAsync(round.GameId);
                if (game != null)
                {
                    var activePlayers = game.Players.Where(p => !p.IsEliminated).Count();
                    var votesCount = await _context.Votes.CountAsync(v => v.RoundId == roundId);
                    var majorityNeeded = (activePlayers / 2) + 1;

                    if (votesCount >= majorityNeeded)
                    {
                        // Calculate voting results and end round
                        var (eliminatedPlayerId, isImpostorEliminated) = await CalculateVotingResultsAsync(roundId);
                        await EndRoundAsync(roundId);
                    }
                }

                return (true, "Vote submitted successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error submitting vote: {ex.Message}");
            }
        }

        public async Task<List<Vote>> GetRoundVotesAsync(Guid roundId)
        {
            return await _context.Votes
                .Include(v => v.Voter)
                .Include(v => v.Target)
                .Where(v => v.RoundId == roundId)
                .ToListAsync();
        }

        public async Task<(Guid? EliminatedPlayerId, bool IsImpostorEliminated)> CalculateVotingResultsAsync(Guid roundId)
        {
            var votes = await GetRoundVotesAsync(roundId);

            if (!votes.Any())
            {
                return (null, false);
            }

            // Count votes for each player
            var voteCount = votes
                .GroupBy(v => v.TargetId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Find player with most votes
            var maxVotes = voteCount.Values.Max();
            var playersWithMaxVotes = voteCount
                .Where(kv => kv.Value == maxVotes)
                .Select(kv => kv.Key)
                .ToList();

            // If tie, no one is eliminated
            if (playersWithMaxVotes.Count > 1)
            {
                return (null, false);
            }

            var eliminatedPlayerId = playersWithMaxVotes.First();

            // Check if eliminated player is impostor
            var eliminatedPlayer = await _context.Players
                .FirstOrDefaultAsync(p => p.Id == eliminatedPlayerId);

            var isImpostorEliminated = eliminatedPlayer?.IsImpostor ?? false;

            // Mark player as eliminated
            if (eliminatedPlayer != null)
            {
                eliminatedPlayer.IsEliminated = true;
                await _context.SaveChangesAsync();
            }

            return (eliminatedPlayerId, isImpostorEliminated);
        }

        #endregion

        #region Game Logic

        public async Task<(bool Success, string Message)> EndRoundAsync(Guid roundId)
        {
            try
            {
                var round = await _context.Rounds
                    .Include(r => r.Game)
                    .FirstOrDefaultAsync(r => r.Id == roundId);

                if (round == null)
                {
                    return (false, "Round not found!");
                }

                round.EndRound();

                // Check if game should end
                var shouldEndGame = await CheckGameEndConditionsAsync(round.GameId);
                if (shouldEndGame)
                {
                    await EndGameAsync(round.GameId);
                }
                else if (round.Game != null)
                {
                    // Move to next round
                    round.Game.RoundNumber++;
                    if (round.Game.RoundNumber <= round.Game.MaxRounds)
                    {
                        await CreateRoundAsync(round.GameId);
                    }
                    else
                    {
                        await EndGameAsync(round.GameId);
                    }
                }

                await _context.SaveChangesAsync();
                return (true, "Round ended successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error ending round: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> EndGameAsync(Guid gameId)
        {
            try
            {
                var game = await GetGameAsync(gameId);
                if (game == null)
                {
                    return (false, "Game not found!");
                }

                game.State = GameObject.GameState.Ended;
                game.EndedAt = DateTime.UtcNow;
                game.UpdateTimestamp();

                await _context.SaveChangesAsync();
                return (true, "Game ended successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Error ending game: {ex.Message}");
            }
        }

        public async Task<bool> CheckGameEndConditionsAsync(Guid gameId)
        {
            var players = await GetGamePlayersAsync(gameId);
            var alivePlayers = players.Where(p => !p.IsEliminated).ToList();
            var aliveImpostors = alivePlayers.Where(p => p.IsImpostor).ToList();
            var aliveCrewmates = alivePlayers.Where(p => !p.IsImpostor).ToList();

            // Game ends if no impostors left or impostors equal/outnumber crewmates
            return !aliveImpostors.Any() || aliveImpostors.Count() >= aliveCrewmates.Count();
        }

        #endregion

        #region Utility Methods

        public string GenerateUniqueLobbyCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public async Task<bool> IsLobbyCodeUniqueAsync(string code)
        {
            return !await _context.Games
                .AnyAsync(g => g.LobbyCode == code && g.State != GameObject.GameState.Ended);
        }

        #endregion

        #region Private Helper Methods

        private async Task AssignImpostorRolesAsync(Guid gameId)
        {
            var players = await GetGamePlayersAsync(gameId);
            var game = await GetGameAsync(gameId);

            if (game == null) return;

            // Reset all players to not impostor
            foreach (var player in players)
            {
                player.IsImpostor = false;
            }

            // Calculate random number of impostors (1-3, but not more than half the players)
            var maxImpostors = Math.Min(3, players.Count / 2);
            var impostorCount = _random.Next(1, maxImpostors + 1);

            // Select random impostor(s)
            var selectedImpostors = players.OrderBy(x => _random.Next()).Take(impostorCount);

            foreach (var impostor in selectedImpostors)
            {
                impostor.IsImpostor = true;
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Question?> GetRandomQuestionAsync()
        {
            var questions = await _context.Questions
                .Where(q => q.IsActive)
                .ToListAsync();

            if (!questions.Any()) return null;

            return questions[_random.Next(questions.Count())];
        }

        private async Task<WordHidden?> GetRandomWordAsync()
        {
            var words = await _context.WordHiddens
                .Where(w => w.IsActive)
                .ToListAsync();

            if (!words.Any()) return null;

            return words[_random.Next(words.Count())];
        }

        #endregion
    }
}
