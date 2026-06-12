using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/images/{imageId}")]
public class GameCompletionController(ICompletionService completionService, ILogger<GameCompletionController> logger) : ControllerBase
{
    private readonly ICompletionService _completionService = completionService;
    private readonly ILogger<GameCompletionController> _logger = logger;

  [HttpPost("completions")]
  [EnableRateLimiting("CompletionCreation")]
    public async Task<IActionResult> CreateCompletion(Guid imageId, [FromBody] CreateGameCompletionDto dto)
    {
      _logger.LogInformation("Creating game completion for player: {PlayerName}", dto.PlayerName);

      var completion = await _completionService.CreateCompletionAsync(imageId, dto);

      _logger.LogInformation("Game completion created with ID: {CompletionId}", completion.Id);
      return CreatedAtAction(nameof(GetCompletionsByImageId), new { imageId }, completion);
    }

    [HttpGet("completions")]
    [EnableRateLimiting("CompletionsView")]
    public async Task<IActionResult> GetCompletionsByImageId(Guid imageId)
    {
      _logger.LogInformation("Getting completions for image ID: {ImageId}", imageId);

      var completions = await _completionService.GetCompletionsByImageIdAsync(imageId);

      _logger.LogInformation("Found {Count} completions for image ID: {ImageId}", completions.Count, imageId);
      return Ok(completions);
    }
}