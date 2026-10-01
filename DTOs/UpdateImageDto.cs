namespace WheresWaldoApi.DTOs;

public class UpdateImageDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? PublicId { get; set; }
    public int? OriginalWidth { get; set; }
    public int? OriginalHeight { get; set; }
}