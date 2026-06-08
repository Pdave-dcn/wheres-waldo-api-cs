using Microsoft.AspNetCore.Mvc;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/characters")]
public class CharacterController: ControllerBase
{
  private readonly ICharacterService _characterService;
  private readonly ILogger<CharacterController> _logger;

  public CharacterController(ICharacterService characterService, ILogger<CharacterController> logger)
  {
    _characterService = characterService;
    _logger = logger;
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetCharacterById(Guid id)
  {
    _logger.LogInformation("Getting character with ID: {CharacterId}", id);

    var character = await _characterService.GetCharacterByIdAsync(id);
    if (character is null)
    {
      _logger.LogWarning("Character with ID: {CharacterId} not found.", id);
      return NotFound();
    }
    _logger.LogInformation("Found character: {CharacterName}", character.CharacterType);
    return Ok(character);
  }

  [HttpPost]
  public async Task<IActionResult> AddCharacter([FromBody] AddCharacterDto dto)
  {
    _logger.LogInformation("Adding new character with name: {CharacterName}", dto.CharacterType);

    var character = await _characterService.AddCharacterAsync(dto);
    
    _logger.LogInformation("Character added with ID: {CharacterId}", character.Id);
    return CreatedAtAction(nameof(GetCharacterById), new { id = character.Id }, character);
  }
}