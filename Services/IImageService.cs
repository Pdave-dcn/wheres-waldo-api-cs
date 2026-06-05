using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public interface IImageService
{
  Task<List<Image>> GetAllImagesAsync();

  Task<Image?> GetImageByIdAsync(Guid id);

  Task<Image> CreateImageAsync(CreateImageDto dto);
}