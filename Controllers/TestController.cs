using Microsoft.AspNetCore.Mvc;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;



[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public TestController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            message = "API is working"
        });
    }

    [HttpGet("character")]
    public IActionResult GetCharacter()
    {
        var character = _characterService.GetCharacter();
        return Ok(character);
    }
    
    [HttpGet("character-name")]
    public IActionResult GetCharacterName()
    {
        var name = _characterService.GetCharacterName();

        return Ok(
            new
            {
                name
            }
        );
    }
}

