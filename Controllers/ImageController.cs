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

    _logger.LogInformation("Found image: {ImageName}", image.Name);

    return Ok(image);
  }

  [HttpPost]
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

}