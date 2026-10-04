using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Models;
using WheresWaldoApi.Exceptions;
using System.Security.Claims;

namespace WheresWaldoApi.Services;

public class CharacterService(AppDbContext context) : ICharacterService
{
    private readonly AppDbContext _context = context;

    public async Task<CharacterDto> GetCharacterByIdAsync(Guid id)
    {
        var character = await _context.Characters.FindAsync(id);
        if (character is null)
            throw new CharacterNotFoundException(id);

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

    public async Task<List<CharacterDto>> GetCharactersByImageIdAsync(Guid id)
    {
        var image = await _context.Images.FindAsync(id)
            ?? throw new ImageNotFoundException(id);

        var characters = await _context.Characters
            .AsNoTracking()
            .Where(c => c.ImageId == id)
            .Select(c => new CharacterDto
            {
                Id = c.Id,
                CharacterType = c.CharacterType.ToString(),
                TargetXRatio = c.TargetXRatio,
                TargetYRatio = c.TargetYRatio,
                ToleranceXRatio = c.ToleranceXRatio,
                ToleranceYRatio = c.ToleranceYRatio,
                ImageId = c.ImageId
            })
            .ToListAsync();

        return characters;

    }

    public async Task<CharacterDto> AddCharacterAsync(AddCharacterDto dto, ClaimsPrincipal user)
    {
        if (!user.IsInRole("Admin"))
        {
            throw new ForbiddenException();
        }
        
        var image = await _context.Images.FindAsync(dto.ImageId)
            ?? throw new ImageNotFoundException(dto.ImageId);

        bool alreadyExists = await _context.Characters.AnyAsync(c => c.ImageId == dto.ImageId && c.CharacterType == dto.CharacterType);

        if (alreadyExists)
            throw new CharacterAlreadyExistsException(dto.CharacterType.ToString(), dto.ImageId);

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

    public async Task<CharacterDto> UpdateCharacterAsync(Guid id, UpdateCharacterDto dto, ClaimsPrincipal user)
    {
        if (!user.IsInRole("Admin"))
        {
            throw new ForbiddenException();
        }

        var character = await _context.Characters.FindAsync(id)
            ?? throw new CharacterNotFoundException(id);

        character.CharacterType = dto.CharacterType ?? character.CharacterType;
        character.TargetXRatio = dto.TargetXRatio ?? character.TargetXRatio;
        character.TargetYRatio = dto.TargetYRatio ?? character.TargetYRatio;
        character.ToleranceXRatio = dto.ToleranceXRatio ?? character.ToleranceXRatio;
        character.ToleranceYRatio = dto.ToleranceYRatio ?? character.ToleranceYRatio;

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