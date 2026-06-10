using FluentValidation;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Validators;

public class AddCharacterDtoValidator : AbstractValidator<AddCharacterDto>
{
  public AddCharacterDtoValidator()
  {
    RuleFor(x => x.CharacterType)
      .IsInEnum()
      .WithMessage("CharacterType must be a valid enum value");

    RuleFor(x => x.TargetXRatio)
      .InclusiveBetween(0, 1);

    RuleFor(x => x.TargetYRatio)
      .InclusiveBetween(0, 1);

    RuleFor(x => x.ToleranceXRatio)
      .InclusiveBetween(0, 1);

    RuleFor(x => x.ToleranceYRatio)
      .InclusiveBetween(0, 1);

    RuleFor(x => x.ImageId)
      .NotEmpty();
  }
}