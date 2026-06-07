namespace WheresWaldoApi.Models;

public enum CharacterType
{
    Waldo,
    Wenda,
    Odlaw,
    Wizard
}


public class Character
{
    public Guid Id { get; set; }

    public CharacterType CharacterType { get; set; }

    public double TargetXRatio { get; set; }

    public double TargetYRatio { get; set; }

    public double ToleranceXRatio{get; set;}

    public double ToleranceYRatio{get; set;}

    public Guid ImageId{get; set;}

    public Image Image {get; set;} = null!;
}