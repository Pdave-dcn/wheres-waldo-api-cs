using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class CharacterServiceDeleteCharacterAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly CharacterService _characterService;

  public CharacterServiceDeleteCharacterAsyncTests()
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

  private async Task<Character> SeedCharacterAsync(Guid imageId, CharacterType type = CharacterType.Waldo)
  {
    var character = new Character
    {
      Id = Guid.NewGuid(),
      CharacterType = type,
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
  public async Task DeleteCharacterAsync_WithExistingCharacter_ShouldRemoveCharacter()
  {
    var image = await SeedImageAsync();
    var character = await SeedCharacterAsync(image.Id);

    await _characterService.DeleteCharacterAsync(character.Id, TestUsers.Admin());

    Assert.Null(await _context.Characters.FindAsync(character.Id));
  }

  [Fact]
  public async Task DeleteCharacterAsync_WithExistingCharacter_ShouldLeaveNoCharacters()
  {
    var image = await SeedImageAsync();
    var character = await SeedCharacterAsync(image.Id);

    await _characterService.DeleteCharacterAsync(character.Id, TestUsers.Admin());

    Assert.Equal(0, await _context.Characters.CountAsync());
  }

  [Fact]
  public async Task DeleteCharacterAsync_WithMultipleCharacters_ShouldDeleteOnlyTargetCharacter()
  {
    var image = await SeedImageAsync();
    var waldo = await SeedCharacterAsync(image.Id, CharacterType.Waldo);
    await SeedCharacterAsync(image.Id, CharacterType.Wizard);

    await _characterService.DeleteCharacterAsync(waldo.Id, TestUsers.Admin());

    Assert.Null(await _context.Characters.FindAsync(waldo.Id));
    Assert.Equal(1, await _context.Characters.CountAsync());
  }

  [Fact]
  public async Task DeleteCharacterAsync_ShouldNotDeleteItsImage()
  {
    var image = await SeedImageAsync();
    var character = await SeedCharacterAsync(image.Id);

    await _characterService.DeleteCharacterAsync(character.Id, TestUsers.Admin());

    Assert.NotNull(await _context.Images.FindAsync(image.Id));
    Assert.Equal(1, await _context.Images.CountAsync());
  }

  [Fact]
  public async Task DeleteCharacterAsync_WithNonexistentId_ShouldThrowCharacterNotFoundException()
  {
    var missingId = Guid.NewGuid();

    var ex = await Assert.ThrowsAsync<CharacterNotFoundException>(
      () => _characterService.DeleteCharacterAsync(missingId, TestUsers.Admin()));

    Assert.Equal(ErrorCodes.CharacterNotFound, ex.Code);
    Assert.Equal(StatusCodes.Status404NotFound, ex.StatusCode);
    Assert.Equal($"Character with ID '{missingId}' was not found.", ex.Message);
  }

  [Fact]
  public async Task DeleteCharacterAsync_WithNonexistentId_ShouldNotRemoveOtherCharacters()
  {
    var image = await SeedImageAsync();
    var character = await SeedCharacterAsync(image.Id);
    var missingId = Guid.NewGuid();

    await Assert.ThrowsAsync<CharacterNotFoundException>(
      () => _characterService.DeleteCharacterAsync(missingId, TestUsers.Admin()));

    Assert.Equal(1, await _context.Characters.CountAsync());
    Assert.NotNull(await _context.Characters.FindAsync(character.Id));
  }

  [Fact]
  public async Task DeleteCharacterAsync_WithNonAdminUser_ShouldThrowForbiddenException()
  {
    var image = await SeedImageAsync();
    var character = await SeedCharacterAsync(image.Id);

    var ex = await Assert.ThrowsAsync<ForbiddenException>(
      () => _characterService.DeleteCharacterAsync(character.Id, TestUsers.RegularUser()));

    Assert.Equal(ErrorCodes.Forbidden, ex.Code);
    Assert.Equal(StatusCodes.Status403Forbidden, ex.StatusCode);
    Assert.NotNull(await _context.Characters.FindAsync(character.Id));
  }
}