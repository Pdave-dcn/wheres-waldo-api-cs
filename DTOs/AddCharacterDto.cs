using System.ComponentModel.DataAnnotations;
using WheresWaldoApi.Models;

namespace WheresWaldoApi.DTOs;

public class AddCharacterDto
{
  [Required]
  public CharacterType CharacterType {get; set;}

  [Required]
  [Range(0.0, 1.0)]
  public double TargetXRatio { get; set; }

  [Required]
  [Range(0.0, 1.0)]
  public double TargetYRatio { get; set; }

  [Required]
  [Range(0.0, 1.0)]
  public double ToleranceXRatio {get; set;}

  [Required]
  [Range(0.0, 1.0)]
  public double ToleranceYRatio {get; set;}

  [Required]
  public Guid ImageId {get; set;}

}
