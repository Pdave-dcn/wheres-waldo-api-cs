namespace WheresWaldoApi.DTOs;

// todo: Update the name to AddImageDto instead
public class AddImageDto
{
  public string Name {get; set;} = string.Empty;

  public string Description {get; set;} = string.Empty;

  public string ImageUrl {get; set;} = string.Empty;

  public string PublicId {get; set;} = string.Empty;

  public int OriginalWidth {get; set;}

  public int OriginalHeight {get; set;}

}