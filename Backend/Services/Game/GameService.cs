using Backend.Areas.Identity.Data;
using Backend.Models;
using Backend.Models.Questions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Backend.Models.GameObject;

namespace Backend.Services.Game
{
    public class GameService : IGameService
    {
        private readonly AuthDbContext _context;
        private readonly Random _random;
        private readonly ILogger<GameService> _logger;

        public GameService(AuthDbContext context, ILogger<GameService> logger)
        {
            _context = context;
            _random = new Random();
            _logger = logger;
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

                // Remove all related entities first to avoid foreign key constraint violations
                var playerVotes = await _context.Votes
                    .Where(v => v.VoterId == player.Id || v.TargetId == player.Id)
                    .ToListAsync();
                _context.Votes.RemoveRange(playerVotes);

                var playerAnswers = await _context.Answers
                    .Where(a => a.PlayerId == player.Id)
                    .ToListAsync();
                _context.Answers.RemoveRange(playerAnswers);

                // Now remove the player
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
                if (game == null)
                {
                    return (false, "Game not found!");
                }

                // Allow ready status change in Lobby (before game starts) and in Game (between rounds)
                if (game.State != GameObject.GameState.Lobby && game.State != GameObject.GameState.Game)
                {
                    return (false, "Cannot change ready status - game is not active!");
                }

                player.IsReady = isReady;
                game.UpdateTimestamp();
                await _context.SaveChangesAsync();

                // Check if all players are ready
                if (isReady)
                {
                    var allReady = game.Players.All(p => p.IsReady);
                    if (allReady)
                    {
                        // Get current round
                        var currentRound = await GetCurrentRoundAsync(gameId);

                        // If in Review state (Question Swap), do NOT allow manual transition
                        // The review will automatically transition after 5 seconds
                        if (currentRound != null && currentRound.State == Round.RoundState.Review)
                        {
                            _logger.LogInformation($"Players ready in Review state, but waiting for 5-second review timer for round {currentRound.Id}");
                            return (true, "Waiting for review period to complete...");
                        }

                        // If game is active and round is ended, start next round
                        if (game.State == GameObject.GameState.Game && currentRound != null && currentRound.State == Round.RoundState.Ended)
                        {
                            _logger.LogInformation($"All players ready! Starting next round for game {gameId}");

                            // Reset all players' ready status
                            foreach (var p in game.Players)
                            {
                                p.IsReady = false;
                            }

                            // End current round
                            await EndRoundAsync(currentRound.Id);

                            // Increment round number
                            game.RoundNumber++;

                            if (game.RoundNumber > game.MaxRounds)
                            {
                                // Game is over - end game (leaderboard will show)
                                await EndGameAsync(gameId);
                                _logger.LogInformation($"Game {gameId} ended! Final scores will be displayed.");
                                return (true, "All players ready - Game ended!");
                            }
                            else
                            {
                                // Create new round
                                await CreateRoundAsync(gameId);
                                return (true, $"All players ready - Round {game.RoundNumber} started!");
                            }
                        }
                    }
                }

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

                // Reset all players' impostor status
                _logger.LogInformation($"Resetting impostor status for {game.Players.Count} players");
                foreach (var player in game.Players)
                {
                    player.IsImpostor = false;
                }

                // Determine number of impostors for this round
                int impostorCount;
                if (game.ImpostorCount == 0)
                {
                    // Random: 0 to all players
                    impostorCount = _random.Next(0, game.Players.Count + 1);
                    _logger.LogInformation($"Random impostor count: {impostorCount} (max: {game.Players.Count}) for Round {game.RoundNumber}");
                }
                else
                {
                    // Fixed number, capped at total players
                    impostorCount = Math.Min(game.ImpostorCount, game.Players.Count);
                    _logger.LogInformation($"Fixed impostor count: {impostorCount} for Round {game.RoundNumber}");
                }

                // Select impostors randomly
                var selectedImpostors = game.Players.OrderBy(x => _random.Next()).Take(impostorCount).ToList();
                foreach (var impostor in selectedImpostors)
                {
                    impostor.IsImpostor = true;
                    _logger.LogInformation($"Selected impostor: Player {impostor.Id} (User: {impostor.UserId}) for Round {game.RoundNumber}");
                }

                await _context.SaveChangesAsync();

                // Use the first impostor for the ImpostorId field (for backward compatibility)
                // If no impostors, use the first player's ID (edge case: 0 impostors)
                var firstImpostor = selectedImpostors.FirstOrDefault();
                var impostorId = firstImpostor?.Id ?? game.Players.FirstOrDefault()?.Id ?? Guid.Empty;

                if (impostorId == Guid.Empty)
                {
                    return (false, null, "No players found to create round!");
                }

                var round = new Round
                {
                    GameId = gameId,
                    ImpostorId = impostorId,
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
                    .ThenInclude(g => g.Players)
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
                    .ThenInclude(g => g.Players)
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
                        // For Question Swap, move to Review state to show answers
                        // For WordHidden, move directly to voting
                        if (game.Type == GameObject.GameType.WordHidden)
                        {
                            // For WordHidden, move to voting phase immediately
                            await StartVotingPhaseAsync(roundId);
                        }
                        else if (game.Type == GameObject.GameType.Questions)
                        {
                            // For Questions, move to Review state
                            round.State = Round.RoundState.Review;
                            await _context.SaveChangesAsync();

                            // Start a background task to automatically transition to Voting after 5 seconds
                            _ = Task.Run(async () =>
                            {
                                await Task.Delay(5000); // Wait 5 seconds

                                try
                                {
                                    // Check if round is still in Review state
                                    var currentRound = await _context.Rounds.FindAsync(round.Id);
                                    if (currentRound != null && currentRound.State == Round.RoundState.Review)
                                    {
                                        _logger.LogInformation($"Auto-transitioning round {round.Id} from Review to Voting after 5 seconds");
                                        await StartVotingPhaseAsync(round.Id);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error auto-transitioning to voting: {ex.Message}");
                                }
                            });
                        }
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

                // Check if ALL active players have voted
                var game = await GetGameAsync(round.GameId);
                if (game != null)
                {
                    var activePlayers = game.Players.Where(p => !p.IsEliminated).Count();
                    var votesCount = await _context.Votes.CountAsync(v => v.RoundId == roundId);

                    // Wait for ALL active players to vote
                    if (votesCount >= activePlayers)
                    {
                        // Calculate voting results
                        var (eliminatedPlayerId, isImpostorEliminated) = await CalculateVotingResultsAsync(roundId);

                        // If tie (no one eliminated), reset votes and restart voting
                        if (eliminatedPlayerId == null)
                        {
                            // Remove all votes for this round
                            var votesToRemove = await _context.Votes
                                .Where(v => v.RoundId == roundId)
                                .ToListAsync();
                            _context.Votes.RemoveRange(votesToRemove);
                            await _context.SaveChangesAsync();

                            return (true, "Voting resulted in a tie! Please vote again.");
                        }
                        else
                        {
                            // Someone was eliminated, end the round
                            await EndRoundAsync(roundId);
                        }
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

            // Award points (NO ONE is actually eliminated from the game)
            if (eliminatedPlayer != null)
            {
                // Don't set IsEliminated = true - players continue in next rounds

                // Award points
                var round = await _context.Rounds
                    .Include(r => r.Game)
                    .ThenInclude(g => g.Players)
                    .FirstOrDefaultAsync(r => r.Id == roundId);

                if (round?.Game != null)
                {
                    if (isImpostorEliminated)
                    {
                        // Crewmates found the impostor - give them 100 points each
                        // Give points to all crewmates (players who are NOT impostors in this round)
                        var crewmates = round.Game.Players.Where(p => !p.IsImpostor).ToList();
                        _logger.LogInformation($"Awarding 100 points to {crewmates.Count} crewmates (impostor was voted)");
                        foreach (var crewmate in crewmates)
                        {
                            _logger.LogInformation($"Player {crewmate.Id}: Score {crewmate.Score} -> {crewmate.Score + 100}");
                            crewmate.Score += 100;
                        }
                    }
                    else
                    {
                        // Wrong player voted - impostor gets 100 points
                        var impostors = round.Game.Players.Where(p => p.IsImpostor).ToList();
                        _logger.LogInformation($"Awarding 100 points to {impostors.Count} impostors (crewmate was voted)");
                        foreach (var impostor in impostors)
                        {
                            _logger.LogInformation($"Impostor {impostor.Id}: Score {impostor.Score} -> {impostor.Score + 100}");
                            impostor.Score += 100;
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return (eliminatedPlayerId, isImpostorEliminated);
        }

        public async Task<(bool Success, string Message, int Points)> GuessWordAsync(Guid roundId, Guid userId, string guessedWord)
        {
            try
            {
                var round = await _context.Rounds
                    .Include(r => r.WordHidden)
                    .Include(r => r.Game)
                        .ThenInclude(g => g.Players)
                    .FirstOrDefaultAsync(r => r.Id == roundId);

                if (round == null)
                {
                    return (false, "Round not found!", 0);
                }

                if (round.WordHidden == null)
                {
                    return (false, "This round doesn't have a word to guess!", 0);
                }

                // Find the player
                var player = round.Game?.Players.FirstOrDefault(p => p.UserId == userId);
                if (player == null)
                {
                    return (false, "Player not found in this game!", 0);
                }

                // Check if player is the impostor who was voted
                if (!player.IsImpostor)
                {
                    return (false, "Only the voted impostor can guess the word!", 0);
                }

                // Check if the word matches (case-insensitive)
                var normalizedGuess = guessedWord.Trim().ToLower();
                var normalizedCorrectWord = round.WordHidden.Word.Trim().ToLower();

                if (normalizedGuess == normalizedCorrectWord)
                {
                    // Correct guess! Award 50 points
                    player.Score += 50;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Player {player.Id} guessed the word correctly! +50 points. Total: {player.Score}");
                    return (true, $"Corect! Cuvântul era '{round.WordHidden.Word}'. +50 puncte!", 50);
                }
                else
                {
                    _logger.LogInformation($"Player {player.Id} guessed incorrectly: '{guessedWord}' (correct: '{round.WordHidden.Word}')");
                    return (false, $"Greșit! Cuvântul corect era '{round.WordHidden.Word}'.", 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error guessing word: {ex.Message}");
                return (false, $"Error: {ex.Message}", 0);
            }
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

                // Reset all players' ready status for next round
                if (round.Game != null)
                {
                    foreach (var player in round.Game.Players)
                    {
                        player.IsReady = false;
                    }
                }

                // Check if game should end
                var shouldEndGame = await CheckGameEndConditionsAsync(round.GameId);
                if (shouldEndGame)
                {
                    await EndGameAsync(round.GameId);
                }
                // Don't automatically create next round - wait for explicit trigger
                // Frontend will show results, word guessing, and "Next Round" button

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
            _logger.LogInformation($"[First Round] Resetting impostor status for {players.Count} players");
            foreach (var player in players)
            {
                player.IsImpostor = false;
            }

            // Determine number of impostors
            int impostorCount;
            if (game.ImpostorCount == 0)
            {
                // Random: 0 to all players
                impostorCount = _random.Next(0, players.Count + 1);
                _logger.LogInformation($"[First Round] Random impostor count: {impostorCount} (max: {players.Count})");
            }
            else
            {
                // Fixed number, capped at total players
                impostorCount = Math.Min(game.ImpostorCount, players.Count);
                _logger.LogInformation($"[First Round] Fixed impostor count: {impostorCount}");
            }

            // Select impostors randomly
            var selectedImpostors = players.OrderBy(x => _random.Next()).Take(impostorCount).ToList();
            foreach (var impostor in selectedImpostors)
            {
                impostor.IsImpostor = true;
                _logger.LogInformation($"[First Round] Selected impostor: Player {impostor.Id} (User: {impostor.UserId})");
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
