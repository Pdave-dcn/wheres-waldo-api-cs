using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;
using System.Security.Claims;

namespace WheresWaldoApi.Services;

public interface IImageService
{
  Task<PageResultDto<ImageListItemDto>> GetAllImagesAsync(string? cursor, int pageSize);

  Task<ImageDetailsDto> GetImageByIdAsync(Guid id);

  Task<Image> AddImageAsync(AddImageDto dto, ClaimsPrincipal user);

  Task <ImageDetailsDto> UpdateImageAsync(Guid id, UpdateImageDto dto, ClaimsPrincipal user);

  Task DeleteImageAsync(Guid id, ClaimsPrincipal user);
}