using WheresWaldoApi.Data;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Services;

public class GuessService(AppDbContext context, ICharacterService characterService): IGuessService
{
  private readonly AppDbContext _context = context;
  private readonly ICharacterService _characterService = characterService;

  public async Task<GuessResultDto> VerifyGuessAsync(Guid imageId, VerifyGuessDto dto)
  {
    var image = await _context.Images.FindAsync(imageId)
      ?? throw new ImageNotFoundException(imageId);

    var characters = await _characterService.GetCharactersByImageIdAsync(imageId);

    foreach (var character in characters)
    {
        if (Math.Abs(dto.XRatio - character.TargetXRatio) <= character.ToleranceXRatio
            && Math.Abs(dto.YRatio - character.TargetYRatio) <= character.ToleranceYRatio)
        {
            return new GuessResultDto { Found = true, CharacterType = character.CharacterType };
        }
    }

    return new GuessResultDto { Found = false, CharacterType = null };
  }
}