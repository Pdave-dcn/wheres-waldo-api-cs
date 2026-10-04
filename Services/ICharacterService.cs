using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;
using System.Security.Claims;


namespace WheresWaldoApi.Services;

public interface ICharacterService
{
    
    Task<CharacterDto> GetCharacterByIdAsync(Guid id);

    Task<List<CharacterDto>> GetCharactersByImageIdAsync(Guid id);

    Task<CharacterDto> AddCharacterAsync(AddCharacterDto dto, ClaimsPrincipal user);

    Task<CharacterDto> UpdateCharacterAsync(Guid id, UpdateCharacterDto dto, ClaimsPrincipal user);
}