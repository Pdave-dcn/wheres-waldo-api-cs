namespace WheresWaldoApi.DTOs;

public class ImageDetailsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int OriginalWidth { get; set; }

    public int OriginalHeight { get; set; }

    public List<CharacterDto> Characters { get; set; } = [];
}