using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Models;
using WheresWaldoApi.Data;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Services;

public class LeaderboardService(AppDbContext context): ILeaderboardService
{
  private readonly AppDbContext _context = context;

  public async Task<List<GameCompletionDto>> GetLeaderboardForImageAsync(Guid imageId)
  {
    var image = await _context.Images.FindAsync(imageId)
      ?? throw new ImageNotFoundException(imageId);

    return await _context.Completions
      .AsNoTracking()
      .Where(c => c.ImageId == imageId)
      .OrderBy(c => c.TimeTaken)
      .Take(10)
      .Select(c => new GameCompletionDto
      {
        Id = c.Id,
        PlayerName = c.PlayerName,
        TimeTaken = c.TimeTaken,
        CompletedAt = c.CompletedAt,
      })
      .ToListAsync();
  }
}