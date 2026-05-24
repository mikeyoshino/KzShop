using FluentValidation;

namespace Ecommerce.Application.Categories.UpdateCategory;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    private const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Category id is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Category name is required.")
            .MaximumLength(120)
            .WithMessage("Category name must be 120 characters or fewer.");

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Category slug is required.")
            .MaximumLength(160)
            .WithMessage("Category slug must be 160 characters or fewer.")
            .Matches(SlugPattern)
            .WithMessage("Category slug must contain only lowercase letters, numbers, and hyphens, with no spaces.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Category description must be 500 characters or fewer.");
    }
}
