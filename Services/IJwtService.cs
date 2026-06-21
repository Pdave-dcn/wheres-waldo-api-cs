using WheresWaldoApi.Models;

namespace WheresWaldoApi.Services;

public interface IJwtService
{
  string GenerateToken(User user);
}