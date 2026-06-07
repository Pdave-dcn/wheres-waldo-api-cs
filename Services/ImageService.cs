using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public class ImageService: IImageService
{
  private readonly AppDbContext _context;

  public ImageService(AppDbContext context)
  {
    _context = context;
  }

  public async Task<List<Image>> GetAllImagesAsync()
  {
    return await _context.Images
      .AsNoTracking()
      .ToListAsync();
  }

  public async Task<ImageDetailsDto?> GetImageByIdAsync(Guid id)
  {
    var image = await _context.Images
        .AsNoTracking()
        .Include(i => i.Characters)
        .FirstOrDefaultAsync(i => i.Id == id);

    if (image is null) return null;

    return new ImageDetailsDto
    {
      Id = image.Id,
      Name = image.Name,
      Description = image.Description,
      ImageUrl = image.ImageUrl,
      OriginalWidth = image.OriginalWidth,
      OriginalHeight = image.OriginalHeight,
      Characters = image.Characters.Select(c => new CharacterDto
      {
        Id = c.Id,
        CharacterType = c.CharacterType.ToString(),
        TargetXRatio = c.TargetXRatio,
        TargetYRatio = c.TargetYRatio,
        ToleranceXRatio = c.ToleranceXRatio,
        ToleranceYRatio = c.ToleranceYRatio,
      }).ToList()
    };
  }

  public async Task<Image> CreateImageAsync(CreateImageDto dto)
  {
    var exists = await _context.Images.FirstOrDefaultAsync(i => i.Name == dto.Name);
    if (exists is not null)
    {
      throw new Exception("Image with the same name already exists");
    }
    
    var image = new Image
    {
      Name = dto.Name,
      Description = dto.Description,
      ImageUrl = dto.ImageUrl,
      PublicId = dto.PublicId,
      OriginalHeight = dto.OriginalHeight,
      OriginalWidth = dto.OriginalWidth
    };

    _context.Images.Add(image);
    await _context.SaveChangesAsync();
    return image;
  }
  
}