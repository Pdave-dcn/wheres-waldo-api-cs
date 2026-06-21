namespace WheresWaldoApi.DTOs;

public class LoginUserDto
{
    public string UsernameOrEmail { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}