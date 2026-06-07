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

  public async Task<List<ImageListItemDto>> GetAllImagesAsync()
  {
    return await _context.Images
      .AsNoTracking()
      .Select(i => new ImageListItemDto
      {
        Id = i.Id,
        Name = i.Name,
        Description = i.Description,
        ImageUrl = i.ImageUrl
      })
      .ToListAsync();
  }

  public async Task<ImageDetailsDto?> GetImageByIdAsync(Guid id)
  {
    return await _context.Images
    .AsNoTracking()
    .Where(i => i.Id == id)
    .Select(i => new ImageDetailsDto
    {
        Id = i.Id,
        Name = i.Name,
        Description = i.Description,
        ImageUrl = i.ImageUrl,
        OriginalWidth = i.OriginalWidth,
        OriginalHeight = i.OriginalHeight,

        Characters = i.Characters.Select(c => new CharacterDto
        {
            Id = c.Id,
            CharacterType = c.CharacterType.ToString(),
            TargetXRatio = c.TargetXRatio,
            TargetYRatio = c.TargetYRatio,
            ToleranceXRatio = c.ToleranceXRatio,
            ToleranceYRatio = c.ToleranceYRatio,
            ImageId = c.ImageId
        }).ToList()
    })
    .FirstOrDefaultAsync();
    
  }

  public async Task<Image> CreateImageAsync(CreateImageDto dto)
  {
    bool exists = await _context.Images.AnyAsync(i => i.Name == dto.Name);
    if (exists)
      throw new InvalidOperationException("Image with the same name already exists");
    
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