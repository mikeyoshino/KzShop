using FluentValidation;

namespace Ecommerce.Application.Studios.CreateStudio;

public class CreateStudioValidator : AbstractValidator<CreateStudioCommand>
{
    public CreateStudioValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(160);
    }
}
