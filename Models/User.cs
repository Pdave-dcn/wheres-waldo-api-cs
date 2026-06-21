namespace WheresWaldoApi.Models;

public enum UserRole
{
  User,
  Admin
}

public class User
{
  public Guid Id { get; set; }
  public string Username { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string PasswordHash { get; set; } = null!;
  public UserRole Role { get; set; } = UserRole.User;
  public DateTime CreatedAt { get; set; }
}