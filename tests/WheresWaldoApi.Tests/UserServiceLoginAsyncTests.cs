using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WheresWaldoApi.Data;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;
using WheresWaldoApi.DTOs;
using WheresWaldoApi.Exceptions;

namespace WheresWaldoApi.Tests;

public class UserServiceLoginAsyncTests : IDisposable
{
  private readonly AppDbContext _context;
  private readonly PasswordHasher<User> _passwordHasher;
  private readonly Mock<IJwtService> _mockJwtService;
  private readonly UserService _userService;

  public UserServiceLoginAsyncTests()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new AppDbContext(options);
    _passwordHasher = new PasswordHasher<User>();
    _mockJwtService = new Mock<IJwtService>();

    _userService = new UserService(
      _context,
      _passwordHasher,
      _mockJwtService.Object);
  }

  public void Dispose()
  {
    _context.Database.EnsureDeleted();
    _context.Dispose();
    GC.SuppressFinalize(this);
  }

  private async Task<User> SeedUserAsync(string username, string email, string password, UserRole role = UserRole.User)
  {
    var user = new User
    {
      Username = username,
      Email = email,
      Role = role,
      CreatedAt = DateTime.UtcNow
    };
    user.PasswordHash = _passwordHasher.HashPassword(user, password);

    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    return user;
  }

  [Fact]
  public async Task LoginAsync_WithValidUsername_ShouldReturnAuthResponseDto()
  {
    await SeedUserAsync("testUser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = "testUser", Password = "Password123!" };

    var result = await _userService.LoginAsync(dto);

    Assert.NotNull(result);
    Assert.Equal("testUser", result.User.Username);
    Assert.Equal(UserRole.User.ToString(), result.User.Role);
  }

  [Fact]
  public async Task LoginAsync_WithValidEmail_ShouldReturnAuthResponseDto()
  {
    await SeedUserAsync("testUser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = "user@example.com", Password = "Password123!" };

    var result = await _userService.LoginAsync(dto);

    Assert.NotNull(result);
    Assert.Equal("user@example.com", result.User.Email);
  }

  [Fact]
  public async Task LoginAsync_WithNonexistentUsernameOrEmail_ShouldThrowInvalidCredentialsException()
  {
    var dto = new LoginUserDto { UsernameOrEmail = "doesNotExist", Password = "Password123!" };

    await Assert.ThrowsAsync<InvalidCredentialsException>(() => _userService.LoginAsync(dto));
  }

  [Fact]
  public async Task LoginAsync_WithWrongPassword_ShouldThrowInvalidCredentialsException()
  {
    await SeedUserAsync("testUser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = "testUser", Password = "WrongPassword!" };

    await Assert.ThrowsAsync<InvalidCredentialsException>(() => _userService.LoginAsync(dto));
  }

  [Fact]
  public async Task LoginAsync_WithEmptyPassword_ShouldThrowInvalidCredentialsException()
  {
    await SeedUserAsync("testUser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = "testUser", Password = "" };

    await Assert.ThrowsAsync<InvalidCredentialsException>(() => _userService.LoginAsync(dto));
  }

  [Fact]
  public async Task LoginAsync_ShouldCallJwtServiceExactlyOnceWithCorrectUser()
  {
    var user = await SeedUserAsync("testUser", "user@example.com", "Password123!");
    _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");

    var dto = new LoginUserDto { UsernameOrEmail = "testUser", Password = "Password123!" };

    var result = await _userService.LoginAsync(dto);

    _mockJwtService.Verify(j => j.GenerateToken(It.Is<User>(u => u.Id == user.Id)), Times.Once);
    Assert.Equal("fake-jwt-token", result.Token);
  }

  [Fact]
  public async Task LoginAsync_WithWrongPassword_ShouldNotCallJwtService()
  {
    await SeedUserAsync("testUser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = "testUser", Password = "WrongPassword!" };

    await Assert.ThrowsAsync<InvalidCredentialsException>(() => _userService.LoginAsync(dto));

    _mockJwtService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_WithNonexistentUser_ShouldNotCallJwtService()
  {
    var dto = new LoginUserDto { UsernameOrEmail = "ghost", Password = "whatever" };

    await Assert.ThrowsAsync<InvalidCredentialsException>(() => _userService.LoginAsync(dto));

    _mockJwtService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
  }

  [Fact]
  public async Task LoginAsync_WithCorrectCredentials_ShouldReturnCorrectRole()
  {
    await SeedUserAsync("adminUser", "admin@example.com", "Password123!", UserRole.Admin);

    var dto = new LoginUserDto { UsernameOrEmail = "adminUser", Password = "Password123!" };

    var result = await _userService.LoginAsync(dto);

    Assert.Equal(UserRole.Admin.ToString(), result.User.Role);
  }

  [Theory]
  [InlineData("TESTUSER")]
  [InlineData("TestUser")]
  public async Task LoginAsync_WithDifferentUsernameCasing_ShouldThrowIfCaseSensitiveMatchIsExpected(string attemptedUsername)
  {
    await SeedUserAsync("testuser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = attemptedUsername, Password = "Password123!" };

    await Assert.ThrowsAsync<InvalidCredentialsException>(() => _userService.LoginAsync(dto));
  }

  [Fact]
  public async Task LoginAsync_WhenMultipleUsersExist_ShouldReturnCorrectMatchingUser()
  {
    await SeedUserAsync("userOne", "one@example.com", "PasswordOne1!");
    await SeedUserAsync("userTwo", "two@example.com", "PasswordTwo2!");

    var dto = new LoginUserDto { UsernameOrEmail = "userTwo", Password = "PasswordTwo2!" };

    var result = await _userService.LoginAsync(dto);

    Assert.Equal("userTwo", result.User.Username);
    Assert.Equal("two@example.com", result.User.Email);
  }

  [Fact]
  public async Task LoginAsync_ResultShouldNotExposePasswordHash()
  {
    await SeedUserAsync("testUser", "user@example.com", "Password123!");

    var dto = new LoginUserDto { UsernameOrEmail = "testUser", Password = "Password123!" };

    var result = await _userService.LoginAsync(dto);

    var props = typeof(UserDto).GetProperties().Select(p => p.Name);
    Assert.DoesNotContain("PasswordHash", props);
  }
}