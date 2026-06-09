using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public interface IImageService
{
  Task<List<ImageListItemDto>> GetAllImagesAsync();

  Task<ImageDetailsDto> GetImageByIdAsync(Guid id);

  Task<Image> CreateImageAsync(CreateImageDto dto);
}