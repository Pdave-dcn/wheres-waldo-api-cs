using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class LeaderboardServiceGetLeaderboardForImageAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly LeaderboardService _leaderboardService;

  public LeaderboardServiceGetLeaderboardForImageAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);

    _leaderboardService = new LeaderboardService(_context);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
    GC.SuppressFinalize(this);
  }

  private async Task<GameCompletion> SeedCompletionAsync(Guid imageId, int timeTaken)
  {
    var completion = new GameCompletion
    {
      PlayerName = "Alice",
      TimeTaken = timeTaken,
      CompletedAt = DateTime.UtcNow,
      ImageId = imageId
    };

    _context.Completions.Add(completion);
    await _context.SaveChangesAsync();
    return completion;
  }

  private async Task<Image> SeedImageAsync(string name = "Waldo Beach")
  {
    var image = new Image
    {
      Id = Guid.NewGuid(),
      Name = name,
      Description = "A busy beach scene.",
      ImageUrl = "https://res.cloudinary.com/demo/image/upload/beach.jpg",
      PublicId = name + "-public-id",
      OriginalWidth = 1000,
      OriginalHeight = 1000
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }

  [Fact]
  public async Task GetLeaderboardForImageAsync_WithNonexistentImageId_ShouldThrowImageNotFoundException()
  {
    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _leaderboardService.GetLeaderboardForImageAsync(Guid.NewGuid()));
  }

  [Fact]
  public async Task GetLeaderboardForImageAsync_WithNoCompletions_ShouldReturnEmptyList()
  {
    var image = await SeedImageAsync();

    var result = await _leaderboardService.GetLeaderboardForImageAsync(image.Id);

    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetLeaderboardForImageAsync_WithMoreThanTenCompletions_ShouldReturnTopTenOrderedByTimeTakenAscending()
  {
    var image = await SeedImageAsync();
    for (int i = 12; i >= 1; i--)
    {
      await SeedCompletionAsync(image.Id, i * 10);
    }

    var result = await _leaderboardService.GetLeaderboardForImageAsync(image.Id);

    Assert.Equal(10, result.Count);
    Assert.Equal(Enumerable.Range(1, 10).Select(i => i * 10).ToArray(), result.Select(c => c.TimeTaken).ToArray());
  }

  [Fact]
  public async Task GetLeaderboardForImageAsync_WithFewerThanTenCompletions_ShouldReturnAll()
  {
    var image = await SeedImageAsync();
    await SeedCompletionAsync(image.Id, 300);
    await SeedCompletionAsync(image.Id, 100);
    await SeedCompletionAsync(image.Id, 200);

    var result = await _leaderboardService.GetLeaderboardForImageAsync(image.Id);

    Assert.Equal(3, result.Count);
    Assert.Equal(new[] { 100, 200, 300 }, result.Select(c => c.TimeTaken).ToArray());
  }

  [Fact]
  public async Task GetLeaderboardForImageAsync_ShouldReturnOnlyCompletionsForThatImage()
  {
    var imageOne = await SeedImageAsync("Image One");
    var imageTwo = await SeedImageAsync("Image Two");
    await SeedCompletionAsync(imageOne.Id, 100);
    await SeedCompletionAsync(imageTwo.Id, 50);
    await SeedCompletionAsync(imageTwo.Id, 200);

    var result = await _leaderboardService.GetLeaderboardForImageAsync(imageOne.Id);

    var dto = Assert.Single(result);
    Assert.Equal(100, dto.TimeTaken);
  }
}