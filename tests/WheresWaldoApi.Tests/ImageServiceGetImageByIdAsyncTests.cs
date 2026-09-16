using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class ImageServiceGetImageByIdAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly ImageService _imageService;

  public ImageServiceGetImageByIdAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);

    _imageService = new ImageService(_context);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
    GC.SuppressFinalize(this);
  }

  private async Task<Image> SeedImageAsync(string name = "Test Image")
  {
    var image = new Image
    {
      Id = Guid.NewGuid(),
      Name = name,
      Description = "A test image.",
      ImageUrl = "https://res.cloudinary.com/demo/image/upload/" + name + ".jpg",
      PublicId = name + "-public-id",
      OriginalWidth = 1024,
      OriginalHeight = 768
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
      TargetXRatio = 0.4,
      TargetYRatio = 0.6,
      ToleranceXRatio = 0.1,
      ToleranceYRatio = 0.1
    };

    _context.Characters.Add(character);
    await _context.SaveChangesAsync();
    return character;
  }

  [Fact]
  public async Task GetImageByIdAsync_WithValidId_ShouldReturnImageDetailsDto()
  {
    var seeded = await SeedImageAsync();

    var result = await _imageService.GetImageByIdAsync(seeded.Id);

    Assert.NotNull(result);
    Assert.Equal(seeded.Id, result.Id);
    Assert.Equal(seeded.Name, result.Name);
    Assert.Equal(seeded.Description, result.Description);
    Assert.Equal(seeded.ImageUrl, result.ImageUrl);
    Assert.Equal(seeded.OriginalWidth, result.OriginalWidth);
    Assert.Equal(seeded.OriginalHeight, result.OriginalHeight);
  }

  [Fact]
  public async Task GetImageByIdAsync_WithNonexistentId_ShouldThrowImageNotFoundException()
  {
    var nonexistentId = Guid.NewGuid();

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _imageService.GetImageByIdAsync(nonexistentId));
  }

  [Fact]
  public async Task GetImageByIdAsync_WithNonexistentId_ExceptionMessageShouldContainId()
  {
    var nonexistentId = Guid.NewGuid();

    var ex = await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _imageService.GetImageByIdAsync(nonexistentId));

    Assert.Equal($"Image '{nonexistentId}' was not found.", ex.Message);
  }

  [Fact]
  public async Task GetImageByIdAsync_WhenMultipleImagesExist_ShouldReturnRequestedImage()
  {
    var first = await SeedImageAsync("First Image");
    var second = await SeedImageAsync("Second Image");

    var result = await _imageService.GetImageByIdAsync(second.Id);

    Assert.Equal(second.Id, result.Id);
    Assert.Equal("Second Image", result.Name);
  }

  [Fact]
  public async Task GetImageByIdAsync_WithCharacters_ShouldReturnCharactersMappedToDtos()
  {
    var image = await SeedImageAsync();
    var waldo = await SeedCharacterAsync(image.Id, CharacterType.Waldo);
    var wizard = await SeedCharacterAsync(image.Id, CharacterType.Wizard);

    var result = await _imageService.GetImageByIdAsync(image.Id);

    Assert.Equal(2, result.Characters.Count);
    Assert.Contains(result.Characters, c => c.Id == waldo.Id && c.CharacterType == CharacterType.Waldo.ToString());
    Assert.Contains(result.Characters, c => c.Id == wizard.Id && c.CharacterType == CharacterType.Wizard.ToString());
  }

  [Fact]
  public async Task GetImageByIdAsync_CharacterDtoShouldMatchSeededFields()
  {
    var image = await SeedImageAsync();
    var seeded = await SeedCharacterAsync(image.Id, CharacterType.Odlaw);

    var result = await _imageService.GetImageByIdAsync(image.Id);

    var dto = Assert.Single(result.Characters);
    Assert.Equal(seeded.Id, dto.Id);
    Assert.Equal(CharacterType.Odlaw.ToString(), dto.CharacterType);
    Assert.Equal(seeded.TargetXRatio, dto.TargetXRatio);
    Assert.Equal(seeded.TargetYRatio, dto.TargetYRatio);
    Assert.Equal(seeded.ToleranceXRatio, dto.ToleranceXRatio);
    Assert.Equal(seeded.ToleranceYRatio, dto.ToleranceYRatio);
    Assert.Equal(seeded.ImageId, dto.ImageId);
  }

  [Fact]
  public async Task GetImageByIdAsync_ShouldNotReturnCharactersFromOtherImages()
  {
    var imageOne = await SeedImageAsync("Image One");
    var imageTwo = await SeedImageAsync("Image Two");
    await SeedCharacterAsync(imageOne.Id, CharacterType.Waldo);
    await SeedCharacterAsync(imageTwo.Id, CharacterType.Wizard);

    var result = await _imageService.GetImageByIdAsync(imageOne.Id);

    var dto = Assert.Single(result.Characters);
    Assert.Equal(CharacterType.Waldo.ToString(), dto.CharacterType);
  }

  [Fact]
  public async Task GetImageByIdAsync_WithNoCharacters_ShouldReturnEmptyCharactersList()
  {
    var image = await SeedImageAsync();

    var result = await _imageService.GetImageByIdAsync(image.Id);

    Assert.NotNull(result.Characters);
    Assert.Empty(result.Characters);
  }
}