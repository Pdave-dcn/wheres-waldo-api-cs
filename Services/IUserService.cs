using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Services;

public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterUserDto dto);

    Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
}