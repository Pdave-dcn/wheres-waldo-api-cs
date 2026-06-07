using Microsoft.AspNetCore.Mvc;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController: ControllerBase
{
  private readonly IImageService _imageService;

  public ImageController(IImageService imageService)
  {
    _imageService = imageService;
  }

  [HttpGet]
  public async Task<IActionResult> GetAllImages()
  {
      var images = await _imageService.GetAllImagesAsync();
    return Ok(images);
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetImageById(Guid id)
  {
    var image = await _imageService.GetImageByIdAsync(id);
    if (image is null)
    {
      return NotFound();
    }

    return Ok(image);
  }

  [HttpPost]
  public async Task<IActionResult> CreateImage(
     [FromBody] CreateImageDto dto
  )
  {
    var image = await _imageService.CreateImageAsync(dto);
    
    return CreatedAtAction(
    nameof(GetImageById),
    new { id = image.Id },
    image
    );
  }

}