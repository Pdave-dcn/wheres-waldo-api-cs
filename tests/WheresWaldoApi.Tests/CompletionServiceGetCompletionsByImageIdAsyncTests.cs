using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class CompletionServiceGetCompletionsByImageIdAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly CompletionService _completionService;

  public CompletionServiceGetCompletionsByImageIdAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);

    _completionService = new CompletionService(_context);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
    GC.SuppressFinalize(this);
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

  private async Task<GameCompletion> SeedCompletionAsync(Guid imageId, int timeTaken, string playerName = "Alice")
  {
    var completion = new GameCompletion
    {
      PlayerName = playerName,
      TimeTaken = timeTaken,
      CompletedAt = DateTime.UtcNow,
      ImageId = imageId
    };

    _context.Completions.Add(completion);
    await _context.SaveChangesAsync();
    return completion;
  }

  [Fact]
  public async Task GetCompletionsByImageIdAsync_WithNonexistentImageId_ShouldThrowImageNotFoundException()
  {
    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _completionService.GetCompletionsByImageIdAsync(Guid.NewGuid()));
  }

  [Fact]
  public async Task GetCompletionsByImageIdAsync_WithNoCompletions_ShouldReturnEmptyList()
  {
    var image = await SeedImageAsync();

    var result = await _completionService.GetCompletionsByImageIdAsync(image.Id);

    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetCompletionsByImageIdAsync_ShouldReturnCompletionsOrderedByTimeTakenAscending()
  {
    var image = await SeedImageAsync();
    await SeedCompletionAsync(image.Id, 300, "Slow");
    await SeedCompletionAsync(image.Id, 100, "Fast");
    await SeedCompletionAsync(image.Id, 200, "Medium");

    var result = await _completionService.GetCompletionsByImageIdAsync(image.Id);

    Assert.Equal(3, result.Count);
    Assert.Equal(new[] { 100, 200, 300 }, result.Select(c => c.TimeTaken).ToArray());
  }

  [Fact]
  public async Task GetCompletionsByImageIdAsync_ShouldReturnOnlyCompletionsForThatImage()
  {
    var imageOne = await SeedImageAsync("Image One");
    var imageTwo = await SeedImageAsync("Image Two");
    await SeedCompletionAsync(imageOne.Id, 100);
    await SeedCompletionAsync(imageTwo.Id, 200);

    var result = await _completionService.GetCompletionsByImageIdAsync(imageOne.Id);

    var dto = Assert.Single(result);
    Assert.Equal(100, dto.TimeTaken);
  }

  [Fact]
  public async Task GetCompletionsByImageIdAsync_WithMoreThanOneHundredCompletions_ShouldCapAtOneHundred()
  {
    var image = await SeedImageAsync();
    for (int i = 1; i <= 101; i++)
    {
      _context.Completions.Add(new GameCompletion
      {
        PlayerName = "Alice",
        TimeTaken = i,
        CompletedAt = DateTime.UtcNow,
        ImageId = image.Id
      });
    }
    await _context.SaveChangesAsync();

    var result = await _completionService.GetCompletionsByImageIdAsync(image.Id);

    Assert.Equal(100, result.Count);
    Assert.Equal(Enumerable.Range(1, 100).ToArray(), result.Select(c => c.TimeTaken).ToArray());
  }

  [Fact]
  public async Task GetCompletionsByImageIdAsync_ReturnedDtoFieldsShouldMatchSeededCompletion()
  {
    var image = await SeedImageAsync();
    var seeded = await SeedCompletionAsync(image.Id, 150, "Carol");

    var result = await _completionService.GetCompletionsByImageIdAsync(image.Id);

    var dto = Assert.Single(result);
    Assert.Equal(seeded.Id, dto.Id);
    Assert.Equal("Carol", dto.PlayerName);
    Assert.Equal(150, dto.TimeTaken);
    Assert.Equal(seeded.CompletedAt, dto.CompletedAt);
  }
}