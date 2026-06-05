using WheresWaldoApi.Models;

namespace WheresWaldoApi.Services;

public class CharacterService : ICharacterService
{
    public string GetCharacterName()
    {
        return "Waldo";
    }

    public Character GetCharacter()
    {
        return new Character
        {
            Id = 2,
            Name = "Odlaw",
            XCoordinate = 0.54,
            YCoordinate = 0.76
        };
    }
}