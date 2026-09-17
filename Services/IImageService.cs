using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public interface IImageService
{
  Task<PageResultDto<ImageListItemDto>> GetAllImagesAsync(string? cursor, int pageSize);

  Task<ImageDetailsDto> GetImageByIdAsync(Guid id);

  Task<Image> AddImageAsync(AddImageDto dto);
}