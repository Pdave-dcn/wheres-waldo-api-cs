using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class GuessServiceVerifyGuessAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly GuessService _guessService;

  public GuessServiceVerifyGuessAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);

    var characterService = new CharacterService(_context);
    _guessService = new GuessService(_context, characterService);
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

  private async Task<Character> SeedCharacterAsync(
      Guid imageId,
      CharacterType type,
      double targetX,
      double targetY,
      double toleranceX = 0.1,
      double toleranceY = 0.1)
  {
    var character = new Character
    {
      Id = Guid.NewGuid(),
      ImageId = imageId,
      CharacterType = type,
      TargetXRatio = targetX,
      TargetYRatio = targetY,
      ToleranceXRatio = toleranceX,
      ToleranceYRatio = toleranceY
    };

    _context.Characters.Add(character);
    await _context.SaveChangesAsync();
    return character;
  }

  [Fact]
  public async Task VerifyGuessAsync_WithNonexistentImage_ShouldThrowImageNotFoundException()
  {
    var dto = new VerifyGuessDto { XRatio = 0.5, YRatio = 0.5 };

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _guessService.VerifyGuessAsync(Guid.NewGuid(), dto));
  }

  [Fact]
  public async Task VerifyGuessAsync_WhenGuessInsideTolerance_ShouldReturnFoundWithMatchingType()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id, CharacterType.Waldo, 0.5, 0.5, 0.1, 0.1);

    var dto = new VerifyGuessDto { XRatio = 0.55, YRatio = 0.45 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.True(result.Found);
    Assert.Equal(CharacterType.Waldo.ToString(), result.CharacterType);
  }

  [Fact]
  public async Task VerifyGuessAsync_WhenGuessOutsideAllTolerances_ShouldReturnNotFound()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id, CharacterType.Waldo, 0.5, 0.5, 0.1, 0.1);

    var dto = new VerifyGuessDto { XRatio = 0.9, YRatio = 0.9 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.False(result.Found);
    Assert.Null(result.CharacterType);
  }

  [Fact]
  public async Task VerifyGuessAsync_WhenGuessExactlyOnToleranceBoundary_ShouldReturnFound()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id, CharacterType.Waldo, 0.5, 0.5, 0.1, 0.1);

    var dto = new VerifyGuessDto { XRatio = 0.6, YRatio = 0.5 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.True(result.Found);
    Assert.Equal(CharacterType.Waldo.ToString(), result.CharacterType);
  }

  [Fact]
  public async Task VerifyGuessAsync_WhenXWithinToleranceButYOutside_ShouldReturnNotFound()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id, CharacterType.Waldo, 0.5, 0.5, 0.1, 0.1);

    var dto = new VerifyGuessDto { XRatio = 0.55, YRatio = 0.75 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.False(result.Found);
  }

  [Fact]
  public async Task VerifyGuessAsync_WhenYWithinToleranceButXOutside_ShouldReturnNotFound()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id, CharacterType.Waldo, 0.5, 0.5, 0.1, 0.1);

    var dto = new VerifyGuessDto { XRatio = 0.75, YRatio = 0.55 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.False(result.Found);
  }

  [Fact]
  public async Task VerifyGuessAsync_WithMultipleCharacters_ShouldReturnTypeWhoseBoxWasHit()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id, CharacterType.Waldo, 0.5, 0.5, 0.1, 0.1);
    await SeedCharacterAsync(image.Id, CharacterType.Wizard, 0.8, 0.8, 0.1, 0.1);

    var dto = new VerifyGuessDto { XRatio = 0.78, YRatio = 0.82 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.True(result.Found);
    Assert.Equal(CharacterType.Wizard.ToString(), result.CharacterType);
  }

  [Fact]
  public async Task VerifyGuessAsync_WhenImageHasNoCharacters_ShouldReturnNotFound()
  {
    var image = await SeedImageAsync();

    var dto = new VerifyGuessDto { XRatio = 0.5, YRatio = 0.5 };

    var result = await _guessService.VerifyGuessAsync(image.Id, dto);

    Assert.False(result.Found);
    Assert.Null(result.CharacterType);
  }
}