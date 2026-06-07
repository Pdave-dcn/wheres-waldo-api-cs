using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public interface IImageService
{
  Task<List<Image>> GetAllImagesAsync();

  Task<ImageDetailsDto?> GetImageByIdAsync(Guid id);

  Task<Image> CreateImageAsync(CreateImageDto dto);
}