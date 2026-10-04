using Microsoft.EntityFrameworkCore;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;
using WheresWaldoApi.Helpers;
using System.Security.Claims;

namespace WheresWaldoApi.Services;

public class ImageService(AppDbContext context) : IImageService
{
  private readonly AppDbContext _context = context;

  public async Task<PageResultDto<ImageListItemDto>> GetAllImagesAsync(string? cursor, int pageSize = 20)
  {
    pageSize = Math.Clamp(pageSize, 1, 100);

    var decodedCursor = CursorHelper.Decode(cursor);

    IQueryable<Image> query = _context.Images
      .AsNoTracking()
      .OrderBy(i => i.CreatedAt)
      .ThenBy(i => i.Id);

    if (decodedCursor is not null)
    {
      query = query.Where( i =>
      i.CreatedAt > decodedCursor.CreatedAt ||
      (i.CreatedAt == decodedCursor.CreatedAt && i.Id > decodedCursor.Id));
    }

    var rows = await query
        .Take(pageSize + 1)
        .Select(i => new
        {
          i.Id,
          i.Name,
          i.Description,
          i.ImageUrl,
          i.CreatedAt
        })
        .ToListAsync();

    bool hasMore = rows.Count > pageSize;
    if (hasMore) rows.RemoveAt(rows.Count - 1);

    string? nextCursor = hasMore
        ? CursorHelper.Encode(new CursorDto(rows[^1].CreatedAt, rows[^1].Id))
        : null;

    return new PageResultDto<ImageListItemDto>
    {
        Items = rows.Select(r => new ImageListItemDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            ImageUrl = r.ImageUrl
        }).ToList(),
        NextCursor = nextCursor,
        HasMore = hasMore
    };

  }

  public async Task<ImageDetailsDto> GetImageByIdAsync(Guid id)
  {
    var image = await _context.Images
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
    .FirstOrDefaultAsync() ?? throw new ImageNotFoundException(id);

    return image;
    
  }

  public async Task<Image> AddImageAsync(AddImageDto dto, ClaimsPrincipal user)
  {
    if(!user.IsInRole("Admin"))
    {
        throw new ForbiddenException();
    }

    bool exists = await _context.Images.AnyAsync(i => i.Name == dto.Name);
    if (exists)
      throw new ImageAlreadyExistsException(dto.Name);
    
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

  public async Task<ImageDetailsDto> UpdateImageAsync(Guid id, UpdateImageDto dto, ClaimsPrincipal user)
  {
    if(!user.IsInRole("Admin"))
    {
        throw new ForbiddenException();
    }

    var image = await _context.Images
        .Include(i => i.Characters)
        .FirstOrDefaultAsync(i => i.Id == id);

    if (image == null)
        throw new ImageNotFoundException(id);
    
    image.Name = dto.Name ?? image.Name;
    image.Description = dto.Description ?? image.Description;
    image.ImageUrl = dto.ImageUrl ?? image.ImageUrl;
    image.PublicId = dto.PublicId ?? image.PublicId;
    image.OriginalHeight = dto.OriginalHeight ?? image.OriginalHeight;
    image.OriginalWidth = dto.OriginalWidth ?? image.OriginalWidth;

    await _context.SaveChangesAsync();

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
            ImageId = c.ImageId
        }).ToList()
    };

  }
  
  public async Task DeleteImageAsync(Guid id, ClaimsPrincipal user)
  {
    if(!user.IsInRole("Admin"))
    {
        throw new ForbiddenException();
    }

    var image = await _context.Images.FindAsync(id) ?? throw new ImageNotFoundException(id);    

    _context.Images.Remove(image);
    await _context.SaveChangesAsync();
  }
}