using Microsoft.AspNetCore.Mvc;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;
using Microsoft.AspNetCore.RateLimiting;

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
  public async Task<IActionResult> GetAllImages()
  {
    _logger.LogInformation("Getting all images.");

    var images = await _imageService.GetAllImagesAsync();

    _logger.LogInformation("Retrieved all images. Count: {ImageCount}", images.Count);
    return Ok(images);
  }

  [HttpGet("{id}")]
  [EnableRateLimiting("ImageSelection")]
  public async Task<IActionResult> GetImageById(Guid id)
  {
    _logger.LogInformation("Getting image with ID: {ImageId}", id);

    var image = await _imageService.GetImageByIdAsync(id);

    _logger.LogInformation("Found image: {ImageName}", image.Name);

    return Ok(image);
  }

  [HttpPost]
  [EnableRateLimiting("ImageUpload")]
  public async Task<IActionResult> AddImage(
     [FromBody] AddImageDto dto
  )
  {
    _logger.LogInformation("Adding new image with name: {ImageName}", dto.Name);

    var image = await _imageService.AddImageAsync(dto);
    
    _logger.LogInformation("Image added with ID: {ImageId}", image.Id);

    return CreatedAtAction(
    nameof(GetImageById),
    new { id = image.Id },
    image
    );
  }

  [HttpPost("{imageId}/guess")]
  [EnableRateLimiting("GuessSubmission")]
  public async Task<IActionResult> SubmitGuess(Guid imageId, [FromBody] VerifyGuessDto dto)
  {
    _logger.LogInformation("Submitting guess for image: {ImageId}", imageId);

    var result = await _guessService.VerifyGuessAsync(imageId, dto);

    _logger.LogInformation("Guess verified. Result: {IsCorrect}", result.Found);
    return Ok(result);
  }

}