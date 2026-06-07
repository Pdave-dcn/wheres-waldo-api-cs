using System.ComponentModel.DataAnnotations;

namespace WheresWaldoApi.DTOs;

// todo: Update the name to AddImageDto instead
public class CreateImageDto
{
  [Required]
  public string Name {get; set;} = string.Empty;

  [Required]
  public string Description {get; set;} = string.Empty;

  [Required]
  public string ImageUrl {get; set;} = string.Empty;

  [Required]
  public string PublicId {get; set;} = string.Empty;

  [Range(1, int.MaxValue)]
  public int OriginalWidth {get; set;}

  [Range(1, int.MaxValue)]
  public int OriginalHeight {get; set;}

}