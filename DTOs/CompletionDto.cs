namespace WheresWaldoApi.DTOs;

public class GameCompletionDto
{
  public int Id { get; set; }

  public string PlayerName { get; set; } = string.Empty;

  public int TimeTaken { get; set; }

  public DateTime CompletedAt { get; set; }
}