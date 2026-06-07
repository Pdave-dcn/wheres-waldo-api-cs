using Microsoft.AspNetCore.Mvc;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/characters")]
public class CharacterController: ControllerBase
{
  private readonly ICharacterService _characterService;

  public CharacterController(ICharacterService characterService)
  {
    _characterService = characterService;
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetCharacterById(Guid id)
  {
      var character = await _characterService.GetCharacterByIdAsync(id);
      if (character is null) return NotFound();
      return Ok(character);
  }

  [HttpPost]
  public async Task<IActionResult> AddCharacter([FromBody] AddCharacterDto dto)
  {
      var character = await _characterService.AddCharacterAsync(dto);
      return CreatedAtAction(nameof(GetCharacterById), new { id = character.Id }, character);
  }
}