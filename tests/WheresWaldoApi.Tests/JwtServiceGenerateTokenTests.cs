using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using WheresWaldoApi.Models;
using WheresWaldoApi.Services;

namespace WheresWaldoApi.Tests;

public class JwtServiceGenerateTokenTests
{
  private const string TestKey = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!";
  private const string TestIssuer = "WheresWaldoApi";
  private const string TestAudience = "WheresWaldoClients";
  private const string TestExpiryMinutes = "60";

  private static IConfiguration BuildConfiguration()
  {
    var values = new Dictionary<string, string?>
    {
      ["Jwt:Key"] = TestKey,
      ["Jwt:Issuer"] = TestIssuer,
      ["Jwt:Audience"] = TestAudience,
      ["Jwt:ExpiryMinutes"] = TestExpiryMinutes
    };

    return new ConfigurationBuilder()
        .AddInMemoryCollection(values)
        .Build();
  }

  private static User BuildUser(string username = "testuser", UserRole role = UserRole.User)
  {
    return new User
    {
      Id = Guid.NewGuid(),
      Username = username,
      Email = username + "@example.com",
      PasswordHash = "hash",
      Role = role,
      CreatedAt = DateTime.UtcNow
    };
  }

  [Fact]
  public void GenerateToken_WithValidUser_ShouldReturnNonEmptyToken()
  {
    var jwtService = new JwtService(BuildConfiguration());

    string token = jwtService.GenerateToken(BuildUser());

    Assert.False(string.IsNullOrWhiteSpace(token));
  }

  [Fact]
  public void GenerateToken_ShouldReturnTokenWithCorrectClaims()
  {
    User user = BuildUser("waldo", UserRole.Admin);
    var jwtService = new JwtService(BuildConfiguration());

    string token = jwtService.GenerateToken(user);

    var principal = ValidateTokenAndGetClaims(token);
    Assert.Equal(user.Id.ToString(), principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
    Assert.Equal(user.Username, principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value);
    Assert.Equal(UserRole.Admin.ToString(), principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
  }

  [Fact]
  public void GenerateToken_ShouldContainConfiguredIssuerAndAudience()
  {
    var jwtService = new JwtService(BuildConfiguration());

    string token = jwtService.GenerateToken(BuildUser());

    var principal = ValidateTokenAndGetClaims(token);
    Assert.Equal(TestIssuer, principal.Claims.FirstOrDefault(c => c.Type == "iss")?.Value);
    Assert.Equal(TestAudience, principal.Claims.FirstOrDefault(c => c.Type == "aud")?.Value);
  }

  [Fact]
  public void GenerateToken_ShouldExpireAroundConfiguredExpiryMinutes()
  {
    var jwtService = new JwtService(BuildConfiguration());
    var beforeCall = DateTime.UtcNow;

    string token = jwtService.GenerateToken(BuildUser());
    var afterCall = DateTime.UtcNow;

    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token);

    Assert.True(jwtToken.ValidTo >= beforeCall.AddMinutes(int.Parse(TestExpiryMinutes)).AddSeconds(-1));
    Assert.True(jwtToken.ValidTo <= afterCall.AddMinutes(int.Parse(TestExpiryMinutes)).AddSeconds(1));
  }

  [Fact]
  public void GenerateToken_ForTwoDifferentUsers_ShouldProduceDifferentTokens()
  {
    var jwtService = new JwtService(BuildConfiguration());

    string tokenOne = jwtService.GenerateToken(BuildUser("alice"));
    string tokenTwo = jwtService.GenerateToken(BuildUser("bob"));

    Assert.NotEqual(tokenOne, tokenTwo);
  }

  private static ClaimsPrincipal ValidateTokenAndGetClaims(string token)
  {
    var handler = new JwtSecurityTokenHandler();
    var validationParameters = new TokenValidationParameters
    {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = TestIssuer,
      ValidAudience = TestAudience,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey))
    };

    return handler.ValidateToken(token, validationParameters, out _);
  }
}