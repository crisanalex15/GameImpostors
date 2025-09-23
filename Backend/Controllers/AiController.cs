using Microsoft.AspNetCore.Mvc;
using Backend.Services.AI;
using Backend.DTO.AiDTO;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly AiService _aiService;
        private readonly ILogger<AiController> _logger;

        public AiController(AiService aiService, ILogger<AiController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Verifică statusul serviciului AI
        /// </summary>
        /// <returns>Statusul serviciului AI</returns>
        [HttpGet("status")]
        public async Task<ActionResult<AiStatusResponse>> GetStatus()
        {
            try
            {
                _logger.LogInformation("Checking AI service status");
                var status = await _aiService.GetStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking AI service status");
                return StatusCode(500, new AiStatusResponse
                {
                    IsHealthy = false,
                    Message = "Eroare internă la verificarea statusului AI",
                    AvailableModels = 0,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Procesează un prompt AI cu parametrul specificat
        /// </summary>
        /// <param name="request">Request-ul cu parametrul X</param>
        /// <returns>Răspunsul de la ChatGPT</returns>
        [HttpPost("prompt")]
        public async Task<ActionResult<AiPromptResponse>> ProcessPrompt([FromBody] AiPromptRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Processing AI prompt for parameter: {X}", request.X);

                // Construim prompt-ul cu template-ul specificat
                var prompt = $"Poți să îmi spui mai multe despre {request.X}";

                var response = await _aiService.ProcessPromptAsync(prompt);

                if (response.Success)
                {
                    _logger.LogInformation("AI prompt processed successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("AI prompt processing failed");
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AI prompt");
                return StatusCode(500, new AiPromptResponse
                {
                    Success = false,
                    Response = "Eroare internă la procesarea prompt-ului",
                    Timestamp = DateTime.UtcNow,
                    Model = "gpt-3.5-turbo"
                });
            }
        }

        /// <summary>
        /// Endpoint simplu pentru testare rapidă
        /// </summary>
        /// <param name="x">Parametrul pentru prompt</param>
        /// <returns>Răspunsul de la ChatGPT</returns>
        [HttpGet("prompt/{x}")]
        public async Task<ActionResult<AiPromptResponse>> ProcessPromptSimple(string x)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    return BadRequest("Parametrul 'x' nu poate fi gol");
                }

                _logger.LogInformation("Processing simple AI prompt for parameter: {X}", x);

                var prompt = $"Poți să îmi spui mai multe despre {x}";
                var response = await _aiService.ProcessPromptAsync(prompt);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing simple AI prompt");
                return StatusCode(500, new AiPromptResponse
                {
                    Success = false,
                    Response = "Eroare internă la procesarea prompt-ului",
                    Timestamp = DateTime.UtcNow,
                    Model = "gpt-3.5-turbo"
                });
            }
        }
    }
}
