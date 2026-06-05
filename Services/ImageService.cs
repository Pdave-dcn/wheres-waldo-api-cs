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

  public async Task<Image?> GetImageByIdAsync(Guid id)
  {
    return await _context.Images.FindAsync(id);
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