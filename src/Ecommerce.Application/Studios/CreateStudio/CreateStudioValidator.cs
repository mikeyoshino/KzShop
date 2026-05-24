using FluentValidation;

namespace Ecommerce.Application.Studios.CreateStudio;

public class CreateStudioValidator : AbstractValidator<CreateStudioCommand>
{
    private const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    public CreateStudioValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(160)
            .Matches(SlugPattern)
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens, with no spaces.");
    }
}
