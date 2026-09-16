using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class CompletionServiceCreateCompletionAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly CompletionService _completionService;

  public CompletionServiceCreateCompletionAsyncTests()
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

  private async Task<Image> SeedImageAsync()
  {
    var image = new Image
    {
      Id = Guid.NewGuid(),
      Name = "Waldo Beach",
      Description = "A busy beach scene.",
      ImageUrl = "https://res.cloudinary.com/demo/image/upload/beach.jpg",
      PublicId = "beach",
      OriginalWidth = 1000,
      OriginalHeight = 1000
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }

  [Fact]
  public async Task CreateCompletionAsync_WithValidDto_ShouldReturnGameCompletionDto()
  {
    var image = await SeedImageAsync();
    var dto = new CreateGameCompletionDto { PlayerName = "Alice", TimeTaken = 120 };
    var beforeCall = DateTime.UtcNow;

    var result = await _completionService.CreateCompletionAsync(image.Id, dto);
    var afterCall = DateTime.UtcNow;

    Assert.NotNull(result);
    Assert.True(result.Id > 0);
    Assert.Equal("Alice", result.PlayerName);
    Assert.Equal(120, result.TimeTaken);
    Assert.True(result.CompletedAt >= beforeCall);
    Assert.True(result.CompletedAt <= afterCall);
  }

  [Fact]
  public async Task CreateCompletionAsync_WithValidDto_ShouldPersistCompletion()
  {
    var image = await SeedImageAsync();
    var dto = new CreateGameCompletionDto { PlayerName = "Bob", TimeTaken = 90 };

    var result = await _completionService.CreateCompletionAsync(image.Id, dto);

    var persisted = await _context.Completions.FindAsync(result.Id);
    Assert.NotNull(persisted);
    Assert.Equal("Bob", persisted!.PlayerName);
    Assert.Equal(90, persisted.TimeTaken);
    Assert.Equal(image.Id, persisted.ImageId);
  }

  [Fact]
  public async Task CreateCompletionAsync_WithNonexistentImageId_ShouldThrowImageNotFoundException()
  {
    var dto = new CreateGameCompletionDto { PlayerName = "Alice", TimeTaken = 120 };

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _completionService.CreateCompletionAsync(Guid.NewGuid(), dto));
  }

  [Fact]
  public async Task CreateCompletionAsync_WithNullPlayerName_ShouldSaveAsAnonymous()
  {
    var image = await SeedImageAsync();
    var dto = new CreateGameCompletionDto { PlayerName = null, TimeTaken = 120 };

    var result = await _completionService.CreateCompletionAsync(image.Id, dto);

    Assert.Equal("Anonymous", result.PlayerName);
  }

  [Fact]
  public async Task CreateCompletionAsync_WithEmptyPlayerName_ShouldSaveAsAnonymous()
  {
    var image = await SeedImageAsync();
    var dto = new CreateGameCompletionDto { PlayerName = "", TimeTaken = 120 };

    var result = await _completionService.CreateCompletionAsync(image.Id, dto);

    Assert.Equal("Anonymous", result.PlayerName);
  }

  [Fact]
  public async Task CreateCompletionAsync_WithWhitespacePlayerName_ShouldSaveAsAnonymous()
  {
    var image = await SeedImageAsync();
    var dto = new CreateGameCompletionDto { PlayerName = "   ", TimeTaken = 120 };

    var result = await _completionService.CreateCompletionAsync(image.Id, dto);

    Assert.Equal("Anonymous", result.PlayerName);
  }

  [Fact]
  public async Task CreateCompletionAsync_WithPaddedPlayerName_ShouldTrimPlayerName()
  {
    var image = await SeedImageAsync();
    var dto = new CreateGameCompletionDto { PlayerName = "  Alice  ", TimeTaken = 120 };

    var result = await _completionService.CreateCompletionAsync(image.Id, dto);

    Assert.Equal("Alice", result.PlayerName);
  }
}