using FluentValidation;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Validators;

public class CreateUserDtoValidator : AbstractValidator<RegisterUserDto>
{
  public CreateUserDtoValidator()
  {
    RuleFor(x => x.Username)
      .NotEmpty().WithMessage("Username is required.")
      .MinimumLength(3).WithMessage("Username must be at least 3 characters long.");

    RuleFor(x => x.Email)
      .NotEmpty().WithMessage("Email is required.")
      .EmailAddress().WithMessage("Invalid email format.");

    RuleFor(x => x.Password)
      .NotEmpty().WithMessage("Password is required.")
      .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
  }
}