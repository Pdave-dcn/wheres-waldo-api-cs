namespace WheresWaldoApi.DTOs;

public class PageResultDto<T>
{
  public List<T> Items { get; set; } = [];
  public string? NextCursor { get; set; }
  public bool HasMore { get; set; }
}