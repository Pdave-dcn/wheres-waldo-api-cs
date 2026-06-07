using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Models;

namespace WheresWaldoApi.Services;

public class CharacterService : ICharacterService
{
    private readonly AppDbContext _context;

    public CharacterService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CharacterDto?> GetCharacterByIdAsync(Guid id)
    {
        var character = await _context.Characters.FindAsync(id);
        if (character is null)
            return null;

        return new CharacterDto
        {
            Id = character.Id,
            CharacterType = character.CharacterType.ToString(),
            TargetXRatio = character.TargetXRatio,
            TargetYRatio = character.TargetYRatio,
            ToleranceXRatio = character.ToleranceXRatio,
            ToleranceYRatio = character.ToleranceYRatio,
            ImageId = character.ImageId
        };
    }

    public async Task<CharacterDto> AddCharacterAsync(AddCharacterDto dto)
    {
        var image = await _context.Images.FindAsync(dto.ImageId);
        if (image is null)
            throw new InvalidOperationException("Image does not exist");

        bool alreadyExists = await _context.Characters.AnyAsync(c => c.ImageId == dto.ImageId && c.CharacterType == dto.CharacterType);

        if (alreadyExists)
            throw new InvalidOperationException($"{dto.CharacterType} is already assigned to this image");

        var character = new Character
        {
            CharacterType = dto.CharacterType,
            TargetXRatio = dto.TargetXRatio,
            TargetYRatio = dto.TargetYRatio,
            ToleranceXRatio = dto.ToleranceXRatio,
            ToleranceYRatio = dto.ToleranceYRatio,
            ImageId = dto.ImageId
        };

        _context.Characters.Add(character);
        await _context.SaveChangesAsync();

        return new CharacterDto
        {
            Id = character.Id,
            CharacterType = character.CharacterType.ToString(),
            TargetXRatio = character.TargetXRatio,
            TargetYRatio = character.TargetYRatio,
            ToleranceXRatio = character.ToleranceXRatio,
            ToleranceYRatio = character.ToleranceYRatio,
            ImageId = character.ImageId
        };
    }
}