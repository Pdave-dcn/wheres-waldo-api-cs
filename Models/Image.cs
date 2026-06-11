namespace WheresWaldoApi.Models;

public class Image
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string PublicId { get; set; } = string.Empty;

    public int OriginalWidth { get; set; }

    public int OriginalHeight { get; set; }

    public ICollection<Character> Characters {get; set;} = [];

    public ICollection<GameCompletion> Completions {get; set;} = [];
}