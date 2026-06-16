using FluentValidation;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Validators;

public class VerifyGuessDtoValidator: AbstractValidator<VerifyGuessDto>
{
  public VerifyGuessDtoValidator()
  {
    RuleFor(x => x.XRatio)
      .NotEmpty()
      .InclusiveBetween(0, 1)
      .WithMessage("XRatio must be between 0 and 1");
    
    RuleFor(x => x.YRatio)
      .NotEmpty()
      .InclusiveBetween(0, 1)
      .WithMessage("YRatio must be between 0 and 1");
  }
}