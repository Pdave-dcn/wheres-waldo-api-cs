using WheresWaldoApi.DTOs;
using WheresWaldoApi.Models;

namespace WheresWaldoApi.Services;

public interface ICompletionService
{
    Task<GameCompletionDto> CreateCompletionAsync(
        Guid imageId,
        CreateGameCompletionDto dto);

    Task<List<GameCompletionDto>> GetCompletionsByImageIdAsync(Guid imageId);
}