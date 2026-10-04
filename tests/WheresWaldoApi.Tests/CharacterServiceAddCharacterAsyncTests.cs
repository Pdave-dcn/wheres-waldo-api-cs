using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;
using Microsoft.AspNetCore.Http;

namespace WheresWaldoApi.Tests;

public class CharacterServiceAddCharacterAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly CharacterService _characterService;

  public CharacterServiceAddCharacterAsyncTests()
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

  private AddCharacterDto BuildDto(Guid imageId, CharacterType type = CharacterType.Waldo) => new()
  {
    CharacterType = type,
    TargetXRatio = 0.5,
    TargetYRatio = 0.5,
    ToleranceXRatio = 0.1,
    ToleranceYRatio = 0.1,
    ImageId = imageId
  };

  [Fact]
  public async Task AddCharacterAsync_WithValidDto_ShouldCreateAndReturnCharacterDto()
  {
    var image = await SeedImageAsync();
    var dto = BuildDto(image.Id);

    var result = await _characterService.AddCharacterAsync(dto, TestUsers.Admin());

    Assert.NotNull(result);
    Assert.NotEqual(Guid.Empty, result.Id);
    Assert.Equal(CharacterType.Waldo.ToString(), result.CharacterType);
    Assert.Equal(dto.TargetXRatio, result.TargetXRatio);
    Assert.Equal(dto.TargetYRatio, result.TargetYRatio);
    Assert.Equal(dto.ToleranceXRatio, result.ToleranceXRatio);
    Assert.Equal(dto.ToleranceYRatio, result.ToleranceYRatio);
    Assert.Equal(image.Id, result.ImageId);
  }

  [Fact]
  public async Task AddCharacterAsync_WithValidDto_ShouldPersistCharacterInDatabase()
  {
    var image = await SeedImageAsync();
    var dto = BuildDto(image.Id);

    var result = await _characterService.AddCharacterAsync(dto, TestUsers.Admin());

    var persisted = await _context.Characters.FindAsync(result.Id);
    Assert.NotNull(persisted);
    Assert.Equal(image.Id, persisted!.ImageId);
    Assert.Equal(CharacterType.Waldo, persisted.CharacterType);
  }

  [Fact]
  public async Task AddCharacterAsync_WithNonexistentImageId_ShouldThrowImageNotFoundException()
  {
    var dto = BuildDto(Guid.NewGuid());

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _characterService.AddCharacterAsync(dto, TestUsers.Admin()));
  }

  [Fact]
  public async Task AddCharacterAsync_WithNonexistentImageId_ShouldNotCreateCharacter()
  {
    var dto = BuildDto(Guid.NewGuid());

    await Assert.ThrowsAsync<ImageNotFoundException>(
      () => _characterService.AddCharacterAsync(dto, TestUsers.Admin()));

    Assert.Empty(_context.Characters);
  }

  [Fact]
  public async Task AddCharacterAsync_WithDuplicateCharacterTypeForSameImage_ShouldThrowCharacterAlreadyExistsException()
  {
    var image = await SeedImageAsync();
    var firstDto = BuildDto(image.Id, CharacterType.Waldo);
    await _characterService.AddCharacterAsync(firstDto, TestUsers.Admin());

    var duplicateDto = BuildDto(image.Id, CharacterType.Waldo);

    await Assert.ThrowsAsync<CharacterAlreadyExistsException>(
      () => _characterService.AddCharacterAsync(duplicateDto, TestUsers.Admin()));
  }

  [Fact]
  public async Task AddCharacterAsync_WithDuplicateCharacterType_ShouldNotCreateSecondCharacter()
  {
    var image = await SeedImageAsync();
    await _characterService.AddCharacterAsync(BuildDto(image.Id, CharacterType.Waldo), TestUsers.Admin());

    await Assert.ThrowsAsync<CharacterAlreadyExistsException>(
      () => _characterService.AddCharacterAsync(BuildDto(image.Id, CharacterType.Waldo), TestUsers.Admin()));

    var count = await _context.Characters.CountAsync(c => c.ImageId == image.Id);
    Assert.Equal(1, count);
  }

  [Fact]
  public async Task AddCharacterAsync_WithSameCharacterTypeOnDifferentImages_ShouldSucceedForBoth()
  {
    var imageOne = await SeedImageAsync();
    var imageTwo = await SeedImageAsync();

    var resultOne = await _characterService.AddCharacterAsync(BuildDto(imageOne.Id, CharacterType.Waldo), TestUsers.Admin());
    var resultTwo = await _characterService.AddCharacterAsync(BuildDto(imageTwo.Id, CharacterType.Waldo), TestUsers.Admin());

    Assert.NotEqual(resultOne.Id, resultTwo.Id);
    Assert.Equal(imageOne.Id, resultOne.ImageId);
    Assert.Equal(imageTwo.Id, resultTwo.ImageId);
  }

  [Fact]
  public async Task AddCharacterAsync_WithDifferentCharacterTypesOnSameImage_ShouldSucceedForBoth()
  {
    var image = await SeedImageAsync();

    var waldo = await _characterService.AddCharacterAsync(BuildDto(image.Id, CharacterType.Waldo), TestUsers.Admin());
    var wizard = await _characterService.AddCharacterAsync(BuildDto(image.Id, CharacterType.Wizard), TestUsers.Admin());

    Assert.NotEqual(waldo.Id, wizard.Id);
    var count = await _context.Characters.CountAsync(c => c.ImageId == image.Id);
    Assert.Equal(2, count);
  }

  [Fact]
  public async Task AddCharacterAsync_WithNonAdminUser_ShouldThrowForbiddenException()
  {
    var image = await SeedImageAsync();
    var dto = BuildDto(image.Id);

    var ex = await Assert.ThrowsAsync<ForbiddenException>(
      () => _characterService.AddCharacterAsync(dto, TestUsers.RegularUser()));

    Assert.Equal(ErrorCodes.Forbidden, ex.Code);
    Assert.Equal(StatusCodes.Status403Forbidden, ex.StatusCode);
  }
}