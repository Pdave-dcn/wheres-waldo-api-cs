using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class ImageServiceDeleteImageAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly ImageService _imageService;

  public ImageServiceDeleteImageAsyncTests()
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

  private async Task<Image> SeedImageAsync()
  {
    var image = new Image
    {
      Id = Guid.NewGuid(),
      Name = "Test Image",
      Description = "A test image for unit testing.",
      ImageUrl = "https://res.cloudinary.com/demo/image/upload/test.jpg",
      PublicId = "test-public-id",
      OriginalWidth = 800,
      OriginalHeight = 600
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }

  private async Task<Character> SeedCharacterAsync(Guid imageId)
  {
    var character = new Character
    {
      Id = Guid.NewGuid(),
      CharacterType = CharacterType.Waldo,
      TargetXRatio = 0.5,
      TargetYRatio = 0.5,
      ToleranceXRatio = 0.1,
      ToleranceYRatio = 0.1,
      ImageId = imageId
    };

    _context.Characters.Add(character);
    await _context.SaveChangesAsync();
    return character;
  }

  [Fact]
  public async Task DeleteImageAsync_WithExistingImage_ShouldRemoveImage()
  {
    var image = await SeedImageAsync();

    await _imageService.DeleteImageAsync(image.Id, TestUsers.Admin());

    Assert.Null(await _context.Images.FindAsync(image.Id));
  }

  [Fact]
  public async Task DeleteImageAsync_WithExistingImage_ShouldLeaveNoImages()
  {
    var image = await SeedImageAsync();

    await _imageService.DeleteImageAsync(image.Id, TestUsers.Admin());

    Assert.Equal(0, await _context.Images.CountAsync());
  }

  [Fact]
  public async Task DeleteImageAsync_WithExistingImage_ShouldCascadeDeleteItsCharacters()
  {
    var image = await SeedImageAsync();
    await SeedCharacterAsync(image.Id);
    await SeedCharacterAsync(image.Id);

    await _imageService.DeleteImageAsync(image.Id, TestUsers.Admin());

    Assert.Equal(0, await _context.Characters.CountAsync());
  }

  [Fact]
  public async Task DeleteImageAsync_WithNonexistentId_ShouldThrowImageNotFoundException()
  {
    var missingId = Guid.NewGuid();

    var ex = await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _imageService.DeleteImageAsync(missingId, TestUsers.Admin()));

    Assert.Equal(ErrorCodes.ImageNotFound, ex.Code);
    Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    Assert.Equal($"Image '{missingId}' was not found.", ex.Message);
  }

  [Fact]
  public async Task DeleteImageAsync_WithNonexistentId_ShouldNotRemoveOtherImages()
  {
    var image = await SeedImageAsync();
    var missingId = Guid.NewGuid();

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _imageService.DeleteImageAsync(missingId, TestUsers.Admin()));

    Assert.Equal(1, await _context.Images.CountAsync());
    Assert.NotNull(await _context.Images.FindAsync(image.Id));
  }

  [Fact]
  public async Task DeleteImageAsync_WithNonAdminUser_ShouldThrowForbiddenException()
  {
    var image = await SeedImageAsync();

    var ex = await Assert.ThrowsAsync<ForbiddenException>(
      () => _imageService.DeleteImageAsync(image.Id, TestUsers.RegularUser()));

    Assert.Equal(ErrorCodes.Forbidden, ex.Code);
    Assert.Equal(StatusCodes.Status403Forbidden, ex.StatusCode);
    Assert.NotNull(await _context.Images.FindAsync(image.Id));
  }
}