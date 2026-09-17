using System.Text;
using System.Text.Json;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Helpers;

public static class CursorHelper
{
  public static string Encode(CursorDto cursor)
  {
    var json = JsonSerializer.Serialize(cursor);
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
  }

  public static CursorDto? Decode(string? cursor)
  {
    if (string.IsNullOrWhiteSpace(cursor))
        return null;

    try
    {
      var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
      return JsonSerializer.Deserialize<CursorDto>(json);
    }
    catch
    {
      return null;
    }
  }
}