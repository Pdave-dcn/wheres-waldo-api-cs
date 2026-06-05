using WheresWaldoApi.Models;

namespace WheresWaldoApi.Services;

public interface ICharacterService
{
    string GetCharacterName();
    Character GetCharacter();
}