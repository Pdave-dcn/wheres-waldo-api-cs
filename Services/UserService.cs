using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;


namespace WheresWaldoApi.Services;

public class UserService(AppDbContext context, PasswordHasher<User> passwordHasher, IJwtService jwtService) : IUserService
{
  private readonly AppDbContext _context = context;
  private readonly PasswordHasher<User> _passwordHasher = passwordHasher;
  private readonly IJwtService _jwtService = jwtService;

  public async Task<UserDto> RegisterAsync(RegisterUserDto dto)
  {
    bool usernameExists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
    if(usernameExists)
      throw new UsernameAlreadyExistsException();

    bool emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
    if(emailExists)
      throw new EmailAlreadyExistsException();

    var user = new User
    {
      Username = dto.Username,
      Email = dto.Email,
      CreatedAt = DateTime.UtcNow
    };

    user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    return new UserDto
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      Role = user.Role.ToString()
    };
  }

  public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
  {
    var user = await _context.Users.FirstOrDefaultAsync( u =>
      u.Email == dto.UsernameOrEmail ||
      u.Username == dto.UsernameOrEmail) ?? throw new InvalidCredentialsException();

    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
    if(result == PasswordVerificationResult.Failed)
      throw new InvalidCredentialsException();

    string token = _jwtService.GenerateToken(user);

    return new AuthResponseDto
    {
      Token = token,
      User = new UserDto
      {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role.ToString()
      }
    };
  }
}