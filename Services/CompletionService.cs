using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Models;
using WheresWaldoApi.Data;
using WheresWaldoApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace WheresWaldoApi.Services;

public class CompletionService(AppDbContext context) : ICompletionService
{
  private readonly AppDbContext _context = context;

  public async Task<GameCompletionDto> CreateCompletionAsync(Guid imageId, [FromBody] CreateGameCompletionDto dto)
  {
    var image = await _context.Images.FindAsync(imageId)
      ?? throw new ImageNotFoundException(imageId);

    string playerName = string.IsNullOrWhiteSpace(dto.PlayerName)
      ? "Anonymous" : dto.PlayerName.Trim();
    
    var completion = new GameCompletion
    {
      PlayerName = playerName,
      TimeTaken = dto.TimeTaken,
      CompletedAt = DateTime.UtcNow,
      ImageId = image.Id
    };
    
    _context.Completions.Add(completion);
    await _context.SaveChangesAsync();

    return new GameCompletionDto
    {
      Id = completion.Id,
      PlayerName = completion.PlayerName,
      TimeTaken = completion.TimeTaken,
      CompletedAt = completion.CompletedAt
    };
  }

  public async Task<List<GameCompletionDto>> GetCompletionsByImageIdAsync(Guid imageId)
  {
    var image = await _context.Images.FindAsync(imageId)
      ?? throw new ImageNotFoundException(imageId);
    
    return await _context.Completions
      .AsNoTracking()
      .Where(c => c.ImageId == imageId)
      .OrderBy(c => c.TimeTaken)
      .Take(100)
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