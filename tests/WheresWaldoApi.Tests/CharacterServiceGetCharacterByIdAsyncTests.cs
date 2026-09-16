using Xunit;
using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class CharacterServiceGetCharacterByIdAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly CharacterService _characterService;

  public CharacterServiceGetCharacterByIdAsyncTests()
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

  private async Task<Character> SeedCharacterAsync(AddCharacterDto dto)
  {
    var character = new Character
    {
      Id = Guid.NewGuid(),
      ImageId = dto.ImageId,
      CharacterType = dto.CharacterType,
      TargetXRatio = dto.TargetXRatio,
      TargetYRatio = dto.TargetYRatio,
      ToleranceXRatio = dto.ToleranceXRatio,
      ToleranceYRatio = dto.ToleranceYRatio
    };

    _context.Characters.Add(character);
    await _context.SaveChangesAsync();
    return character;
  }

  [Fact]
  public async Task GetCharacterByIdAsync_WithValidId_ShouldReturnCharacterDto()
  {
    var dto = new AddCharacterDto
    {
        CharacterType = CharacterType.Waldo,
        TargetXRatio = 0.5,
        TargetYRatio = 0.5,
        ToleranceXRatio = 0.1,
        ToleranceYRatio = 0.1,
        ImageId = Guid.NewGuid()
    };

    var seededCharacter = await SeedCharacterAsync(dto);

    var result = await _characterService.GetCharacterByIdAsync(seededCharacter.Id);

    Assert.NotNull(result);
    Assert.Equal(seededCharacter.Id, result.Id);
    Assert.Equal(seededCharacter.CharacterType.ToString(), result.CharacterType);
    Assert.Equal(seededCharacter.TargetXRatio, result.TargetXRatio);
    Assert.Equal(seededCharacter.TargetYRatio, result.TargetYRatio);
    Assert.Equal(seededCharacter.ToleranceXRatio, result.ToleranceXRatio);
    Assert.Equal(seededCharacter.ToleranceYRatio, result.ToleranceYRatio);
    Assert.Equal(seededCharacter.ImageId, result.ImageId);
  }

  [Fact]
public async Task GetCharacterByIdAsync_WithNonexistentId_ShouldThrowCharacterNotFoundException()
{
  var nonexistentId = Guid.NewGuid();

  await Assert.ThrowsAsync<CharacterNotFoundException>(
    () => _characterService.GetCharacterByIdAsync(nonexistentId));
}

[Fact]
public async Task GetCharacterByIdAsync_WithEmptyGuid_ShouldThrowCharacterNotFoundException()
{
  await Assert.ThrowsAsync<CharacterNotFoundException>(
    () => _characterService.GetCharacterByIdAsync(Guid.Empty));
}

[Fact]
public async Task GetCharacterByIdAsync_WhenMultipleCharactersExist_ShouldReturnCorrectOne()
{
  var dtoOne = new AddCharacterDto
  {
    CharacterType = CharacterType.Waldo,
    TargetXRatio = 0.2,
    TargetYRatio = 0.2,
    ToleranceXRatio = 0.05,
    ToleranceYRatio = 0.05,
    ImageId = Guid.NewGuid()
  };
  var dtoTwo = new AddCharacterDto
  {
    CharacterType = CharacterType.Wizard,
    TargetXRatio = 0.7,
    TargetYRatio = 0.7,
    ToleranceXRatio = 0.1,
    ToleranceYRatio = 0.1,
    ImageId = Guid.NewGuid()
  };

  await SeedCharacterAsync(dtoOne);
  var seededTwo = await SeedCharacterAsync(dtoTwo);

  var result = await _characterService.GetCharacterByIdAsync(seededTwo.Id);

  Assert.Equal(seededTwo.Id, result.Id);
  Assert.Equal(CharacterType.Wizard.ToString(), result.CharacterType);
}

[Fact]
public async Task GetCharacterByIdAsync_ResultCharacterTypeShouldBeStringNotEnum()
{
  var dto = new AddCharacterDto
  {
    CharacterType = CharacterType.Waldo,
    TargetXRatio = 0.5,
    TargetYRatio = 0.5,
    ToleranceXRatio = 0.1,
    ToleranceYRatio = 0.1,
    ImageId = Guid.NewGuid()
  };

  var seededCharacter = await SeedCharacterAsync(dto);

  var result = await _characterService.GetCharacterByIdAsync(seededCharacter.Id);

  Assert.IsType<string>(result.CharacterType);
  Assert.Equal("Waldo", result.CharacterType);
}

[Fact]
public async Task GetCharacterByIdAsync_WithNonexistentId_ExceptionMessageShouldContainId()
{
  var nonexistentId = Guid.NewGuid();

  var ex = await Assert.ThrowsAsync<CharacterNotFoundException>(
    () => _characterService.GetCharacterByIdAsync(nonexistentId));

  Assert.Equal($"Character with ID '{nonexistentId}' was not found.", ex.Message);
}
}