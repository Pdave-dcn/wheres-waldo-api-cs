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

public class UserServiceRegisterAsyncTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly UserService _userService;

    public UserServiceRegisterAsyncTests()
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
    }

    [Fact]
    public async Task RegisterAsync_WithValidUser_ShouldCreateUserAndReturnUserDto()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _userService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(registerDto.Username, result.Username);
        Assert.Equal(registerDto.Email, result.Email);
        Assert.Equal(UserRole.User.ToString(), result.Role);

        var savedUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == registerDto.Username);
        Assert.NotNull(savedUser);
        Assert.Equal(1, await _context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ShouldThrowUsernameAlreadyExistsException()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Username = "existinguser",
            Email = "taken@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var registerDto = new RegisterUserDto
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UsernameAlreadyExistsException>(
            () => _userService.RegisterAsync(registerDto));

        Assert.Equal("Username already exists.", exception.Message);
        Assert.Equal(1, await _context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowEmailAlreadyExistsException()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Username = "takenuser",
            Email = "existing@example.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var registerDto = new RegisterUserDto
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EmailAlreadyExistsException>(
            () => _userService.RegisterAsync(registerDto));

        Assert.Equal("Email already exists.", exception.Message);
        Assert.Equal(1, await _context.Users.CountAsync());
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPasswordUsingPasswordHasher()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "PlainPassword123!"
        };

        // Act
        await _userService.RegisterAsync(registerDto);

        // Assert
        var savedUser = await _context.Users.SingleAsync();
        Assert.False(string.IsNullOrEmpty(savedUser.PasswordHash));
        Assert.NotEqual(registerDto.Password, savedUser.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var beforeCall = DateTime.UtcNow;

        // Act
        await _userService.RegisterAsync(registerDto);
        var afterCall = DateTime.UtcNow;

        // Assert
        var savedUser = await _context.Users.SingleAsync();
        Assert.True(savedUser.CreatedAt >= beforeCall);
        Assert.True(savedUser.CreatedAt <= afterCall);
    }

    [Fact]
    public async Task RegisterAsync_ShouldSetDefaultRoleToUser()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _userService.RegisterAsync(registerDto);

        // Assert
        Assert.Equal(UserRole.User.ToString(), result.Role);

        var savedUser = await _context.Users.SingleAsync();
        Assert.Equal(UserRole.User, savedUser.Role);
    }

    [Fact]
    public async Task RegisterAsync_ReturnedUserDtoShouldHaveIdFromCreatedUser()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await _userService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        var savedUser = await _context.Users.SingleAsync();
        Assert.Equal(savedUser.Id, result.Id);
    }
}
