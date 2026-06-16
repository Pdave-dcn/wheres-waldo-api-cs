using WheresWaldoApi.Models;

namespace WheresWaldoApi.DTOs;

public class GuessResultDto
{
  public bool Found {get; set;}

  public string? CharacterType {get; set;}
}