using FluentValidation;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Validators;

public class CreateGameCompletionValidator : AbstractValidator<CreateGameCompletionDto>
{
  public CreateGameCompletionValidator()
  {
    RuleFor(x => x.PlayerName)
        .NotEmpty()
        .MaximumLength(100);

    RuleFor(x => x.TimeTaken)
        .GreaterThan(0);

    RuleFor(x => x.ImageId)
        .NotEmpty();
  }
}