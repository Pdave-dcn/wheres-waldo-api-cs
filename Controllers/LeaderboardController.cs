using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/images/{imageId}")]
public class LeaderboardController(ILeaderboardService leaderboardService, ILogger logger) : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService = leaderboardService;
    private readonly ILogger _logger = logger;

    [HttpGet("leaderboard")]
    [EnableRateLimiting("LeaderboardView")]
    public async Task<IActionResult> GetLeaderboardForImage(Guid imageId)
    {
      _logger.LogInformation("Getting leaderboard for image ID: {ImageId}", imageId);

      var leaderboard = await _leaderboardService.GetLeaderboardForImageAsync(imageId);

      _logger.LogInformation("Found {Count} entries in leaderboard for image ID: {ImageId}", leaderboard.Count, imageId);
      return Ok(leaderboard);
    }
}