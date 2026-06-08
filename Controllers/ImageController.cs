using Microsoft.AspNetCore.Mvc;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController: ControllerBase
{
  private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

  public ImageController(IImageService imageService, ILogger<ImageController> logger)
  {
    _imageService = imageService;
    _logger = logger;
  }

  [HttpGet]
  public async Task<IActionResult> GetAllImages()
  {
    _logger.LogInformation("Getting all images.");

    var images = await _imageService.GetAllImagesAsync();

    _logger.LogInformation("Retrieved all images. Count: {ImageCount}", images.Count);
    return Ok(images);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetImageById(Guid id)
  {
    _logger.LogInformation("Getting image with ID: {ImageId}", id);

    var image = await _imageService.GetImageByIdAsync(id);
    if (image is null)
    {
      _logger.LogWarning("Image with ID: {ImageId} not found.", id);
      return NotFound();
    }

    _logger.LogInformation("Found image: {ImageName}", image.Name);

    return Ok(image);
  }

  [HttpPost]
  public async Task<IActionResult> CreateImage(
     [FromBody] CreateImageDto dto
  )
  {
    _logger.LogInformation("Creating new image with name: {ImageName}", dto.Name);

    var image = await _imageService.CreateImageAsync(dto);
    
    _logger.LogInformation("Image created with ID: {ImageId}", image.Id);

    return CreatedAtAction(
    nameof(GetImageById),
    new { id = image.Id },
    image
    );
  }

}