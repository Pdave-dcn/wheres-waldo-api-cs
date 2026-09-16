using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class CharacterServiceGetCharactersByImageIdAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly CharacterService _characterService;

  public CharacterServiceGetCharactersByImageIdAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);

    _characterService = new CharacterService(_context);
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
      Name = "Test Image",
      Description = "A test image for unit testing.",
      ImageUrl = "https://example.com/test-image.jpg",
      PublicId = "test-image-public-id",
      OriginalWidth = 800,
      OriginalHeight = 600
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }

  private async Task<Character> SeedCharacterAsync(Guid imageId, CharacterType type)
  {
    var character = new Character
    {
      Id = Guid.NewGuid(),
      ImageId = imageId,
      CharacterType = type,
      TargetXRatio = 0.5,
      TargetYRatio = 0.5,
      ToleranceXRatio = 0.1,
      ToleranceYRatio = 0.1
    };

    _context.Characters.Add(character);
    await _context.SaveChangesAsync();
    return character;
  }

  [Fact]
  public async Task GetCharactersByImageIdAsync_WithNonexistentImageId_ShouldThrowImageNotFoundException()
  {
    var nonexistentId = Guid.NewGuid();

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _characterService.GetCharactersByImageIdAsync(nonexistentId));
  }

  [Fact]
  public async Task GetCharactersByImageIdAsync_WithValidImageIdButNoCharacters_ShouldReturnEmptyList()
  {
    var image = await SeedImageAsync();

    var result = await _characterService.GetCharactersByImageIdAsync(image.Id);

    Assert.NotNull(result);
    Assert.Empty(result);
  }

  [Fact]
  public async Task GetCharactersByImageIdAsync_WithValidImageId_ShouldReturnAllMatchingCharacters()
  {
    var image = await SeedImageAsync();

    await SeedCharacterAsync(image.Id, CharacterType.Waldo);
    await SeedCharacterAsync(image.Id, CharacterType.Wizard);

    var result = await _characterService.GetCharactersByImageIdAsync(image.Id);

    Assert.Equal(2, result.Count);
    Assert.Contains(result, c => c.CharacterType == CharacterType.Waldo.ToString());
    Assert.Contains(result, c => c.CharacterType == CharacterType.Wizard.ToString());
  }

  [Fact]
  public async Task GetCharactersByImageIdAsync_ShouldNotReturnCharactersFromOtherImages()
  {
    var imageOne = await SeedImageAsync();
    var imageTwo = await SeedImageAsync();

    await SeedCharacterAsync(imageOne.Id, CharacterType.Waldo);
    await SeedCharacterAsync(imageTwo.Id, CharacterType.Wizard);

    var result = await _characterService.GetCharactersByImageIdAsync(imageOne.Id);

    Assert.Single(result);
    Assert.Equal(CharacterType.Waldo.ToString(), result[0].CharacterType);
  }

  [Fact]
  public async Task GetCharactersByImageIdAsync_ReturnedDtosShouldMatchSeededCharacterFields()
  {
    var image = await SeedImageAsync();
    var seeded = await SeedCharacterAsync(image.Id, CharacterType.Waldo);

    var result = await _characterService.GetCharactersByImageIdAsync(image.Id);

    var dto = Assert.Single(result);
    Assert.Equal(seeded.Id, dto.Id);
    Assert.Equal(seeded.CharacterType.ToString(), dto.CharacterType);
    Assert.Equal(seeded.TargetXRatio, dto.TargetXRatio);
    Assert.Equal(seeded.TargetYRatio, dto.TargetYRatio);
    Assert.Equal(seeded.ToleranceXRatio, dto.ToleranceXRatio);
    Assert.Equal(seeded.ToleranceYRatio, dto.ToleranceYRatio);
    Assert.Equal(seeded.ImageId, dto.ImageId);
  }
}