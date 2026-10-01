using WheresWaldoApi.Models;

namespace WheresWaldoApi.DTOs;

public class UpdateCharacterDto
{
  public CharacterType? CharacterType {get; set;}

  public double? TargetXRatio { get; set; }

  public double? TargetYRatio { get; set; }

  public double? ToleranceXRatio { get; set; }

  public double? ToleranceYRatio { get; set; }
  public string? ImageId { get; set; }

}