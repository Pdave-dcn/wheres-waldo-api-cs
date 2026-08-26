namespace WheresWaldoApi.Models;

public class GameCompletion
{
    public int Id { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public int TimeTaken { get; set; }

    public DateTime CompletedAt { get; set; }

    public Guid ImageId { get; set; }

    public Image Image { get; set; } = null!;
}