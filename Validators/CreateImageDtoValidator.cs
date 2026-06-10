using FluentValidation;
using WheresWaldoApi.DTOs;

namespace WheresWaldoApi.Validators;

public class CreateImageDtoValidator: AbstractValidator<CreateImageDto>
{
  public CreateImageDtoValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(x => x.Description)
      .NotEmpty();

    RuleFor(x => x.ImageUrl)
      .NotEmpty()
      .Must(url => url.Contains("res.cloudinary.com"))
      .WithMessage("Image url must be a Cloudinary url");

    RuleFor(x => x.PublicId)
      .NotEmpty();

    RuleFor(x => x.OriginalWidth)
      .GreaterThan(0);

    RuleFor(x => x.OriginalHeight)
      .GreaterThan(0);
  }
}