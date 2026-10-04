using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController(IImageService imageService, IGuessService guessService, ILogger<ImageController> logger) : ControllerBase
{
  private readonly IImageService _imageService = imageService;
  private readonly IGuessService _guessService = guessService;
  private readonly ILogger<ImageController> _logger = logger;

  [HttpGet]
  [EnableRateLimiting("ImageSelection")]
  public async Task<ActionResult<PageResultDto<ImageListItemDto>>> GetAllImages(
    [FromQuery] string? cursor = null,
    [FromQuery] int limit = 20
  )
  {
    _logger.LogInformation("Getting all images.");

    var result = await _imageService.GetAllImagesAsync(cursor, limit);

    _logger.LogInformation("Retrieved all images. Count: {ImageCount}", result.Items.Count);
    return Ok(result);
  }

  [HttpGet("{id}")]
  [EnableRateLimiting("ImageSelection")]
  public async Task<ActionResult<ImageDetailsDto>> GetImageById(Guid id)
  {
    _logger.LogInformation("Getting image with ID: {ImageId}", id);

    var image = await _imageService.GetImageByIdAsync(id);

    _logger.LogInformation("Found image: {ImageName}", image.Name);

    return Ok(image);
  }

  [Authorize]
  [HttpPost]
  [EnableRateLimiting("ImageUpload")]
  public async Task<IActionResult> AddImage(
     [FromBody] AddImageDto dto
  )
  {
    _logger.LogInformation("Adding new image with name: {ImageName}", dto.Name);

    var image = await _imageService.AddImageAsync(dto, User);
    
    _logger.LogInformation("Image added with ID: {ImageId}", image.Id);

    return CreatedAtAction(
    nameof(GetImageById),
    new { id = image.Id },
    image
    );
  }

  [Authorize]
  [HttpPut("{id}")]
  [EnableRateLimiting("ImageUpdate")]
  public async Task<ActionResult<ImageDetailsDto>> UpdateImage(Guid id, [FromBody] UpdateImageDto dto)
  {
    _logger.LogInformation("Updating image with ID: {ImageId}", id);

    var updatedImage = await _imageService.UpdateImageAsync(id, dto, User);

    _logger.LogInformation("Image updated: {ImageName}", updatedImage.Name);

    return updatedImage;
  }

  [HttpPost("{id}/guess")]
  [EnableRateLimiting("GuessSubmission")]
  public async Task<IActionResult> SubmitGuess(Guid id, [FromBody] VerifyGuessDto dto)
  {
    _logger.LogInformation("Submitting guess for image: {id}", id);

    var result = await _guessService.VerifyGuessAsync(id, dto);

    _logger.LogInformation("Guess verified. Result: {IsCorrect}", result.Found);
    return Ok(result);
  }

}