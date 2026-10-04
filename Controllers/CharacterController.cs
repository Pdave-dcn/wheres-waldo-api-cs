using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/characters")]
public class CharacterController(ICharacterService characterService, ILogger<CharacterController> logger) : ControllerBase
{
  private readonly ICharacterService _characterService = characterService;
  private readonly ILogger<CharacterController> _logger = logger;

  [HttpGet("{id}")]
  [EnableRateLimiting("CharacterSelection")]
  public async Task<IActionResult> GetCharacterById(Guid id)
  {
    _logger.LogInformation("Getting character with ID: {CharacterId}", id);

    var character = await _characterService.GetCharacterByIdAsync(id);
  
    _logger.LogInformation("Found character: {CharacterName}", character.CharacterType);
    return Ok(character);
  }

  [Authorize]
  [HttpPost]
  [EnableRateLimiting("CharacterAddition")]
  public async Task<IActionResult> AddCharacter([FromBody] AddCharacterDto dto)
  {
    _logger.LogInformation("Adding new character with name: {CharacterName}", dto.CharacterType);

    var character = await _characterService.AddCharacterAsync(dto, User);
    
    _logger.LogInformation("Character added with ID: {CharacterId}", character.Id);
    return CreatedAtAction(nameof(GetCharacterById), new { id = character.Id }, character);
  }

  [Authorize]
  [HttpPut("{id}")]
  [EnableRateLimiting("CharacterUpdate")]
  public async Task<IActionResult> UpdateCharacter(Guid id, [FromBody] UpdateCharacterDto dto)
  {
    _logger.LogInformation("Updating character with ID: {CharacterId}", id);

    var character = await _characterService.UpdateCharacterAsync(id, dto, User);
    
    _logger.LogInformation("Character updated with ID: {CharacterId}", character.Id);
    return Ok(character);
  }

  [Authorize]
  [HttpDelete("{id}")]
  [EnableRateLimiting("CharacterDeletion")]
  public async Task<IActionResult> DeleteCharacter(Guid id)
  {
    _logger.LogInformation("Deleting character with ID: {CharacterId}", id);

    await _characterService.DeleteCharacterAsync(id, User);
    
    _logger.LogInformation("Character deleted with ID: {CharacterId}", id);
    return NoContent();
  }
}