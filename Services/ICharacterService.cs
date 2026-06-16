using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;


namespace WheresWaldoApi.Services;

public interface ICharacterService
{
    
    Task<CharacterDto> GetCharacterByIdAsync(Guid id);

    Task<List<CharacterDto>> GetCharactersByImageIdAsync(Guid id);

    Task<CharacterDto> AddCharacterAsync(AddCharacterDto dto);
}