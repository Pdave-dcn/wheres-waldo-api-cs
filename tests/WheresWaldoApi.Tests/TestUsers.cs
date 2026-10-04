using System.Security.Claims;

namespace WheresWaldoApi.Tests;

public static class TestUsers
{
  public static ClaimsPrincipal Admin() => new(new ClaimsIdentity(new[]
  {
    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.Name, "admin"),
    new Claim(ClaimTypes.Role, "Admin")
  }, "Test"));

  public static ClaimsPrincipal RegularUser() => new(new ClaimsIdentity(new[]
  {
    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.Name, "regular"),
    new Claim(ClaimTypes.Role, "User")
  }, "Test"));
}