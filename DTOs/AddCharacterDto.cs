using WheresWaldoApi.Models;

namespace WheresWaldoApi.DTOs;

public class AddCharacterDto
{
  public CharacterType CharacterType {get; set;}

  public double TargetXRatio { get; set; }

  public double TargetYRatio { get; set; }

  public double ToleranceXRatio {get; set;}

  public double ToleranceYRatio {get; set;}

  public Guid ImageId {get; set;}

}
