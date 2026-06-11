using WheresWaldoApi.DTOs;
using WheresWaldoApi.Models;

namespace WheresWaldoApi.Services;

public interface ILeaderboardService
{
    Task<List<GameCompletionDto>> GetLeaderboardForImageAsync(Guid imageId);
}