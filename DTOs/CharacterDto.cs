namespace WheresWaldoApi.DTOs;

public class CharacterDto
{
    public Guid Id { get; set; }
    public string CharacterType { get; set; } = string.Empty;
    public double TargetXRatio { get; set; }
    public double TargetYRatio { get; set; }
    public double ToleranceXRatio { get; set; }
    public double ToleranceYRatio { get; set; }
    public Guid ImageId { get; set; }
}