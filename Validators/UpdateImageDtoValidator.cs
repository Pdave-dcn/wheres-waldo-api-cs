using WheresWaldoApi.DTOs;
using FluentValidation;

namespace WheresWaldoApi.Validators;

public class UpdateImageDtoValidator : AbstractValidator<UpdateImageDto>
{
    public UpdateImageDtoValidator()
    {
        RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(x => x.Description)
      .NotEmpty();

    RuleFor(x => x.ImageUrl)
      .NotEmpty()
      .Must(url => url is not null && url.Contains("res.cloudinary.com"))
      .WithMessage("Image url must be a Cloudinary url");

    RuleFor(x => x.PublicId)
      .NotEmpty();

    RuleFor(x => x.OriginalWidth)
      .GreaterThan(0);

    RuleFor(x => x.OriginalHeight)
      .GreaterThan(0);
    }
}