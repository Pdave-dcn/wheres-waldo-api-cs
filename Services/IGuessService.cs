using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public interface IGuessService
{
  Task<GuessResultDto> VerifyGuessAsync(Guid imageId, VerifyGuessDto dto);
}