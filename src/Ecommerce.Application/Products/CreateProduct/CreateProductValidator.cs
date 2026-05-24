using FluentValidation;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Products.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    private const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";

    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(SlugPattern)
            .WithMessage("Product slug must contain only lowercase letters, numbers, and hyphens, with no spaces.");

        RuleFor(x => x.ShortDescription)
            .MaximumLength(500);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.StudioId)
            .NotEmpty();

        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.PriceAmount)
            .GreaterThan(0m);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(x => x.Scale)
            .MaximumLength(64);

        RuleFor(x => x.Materials)
            .MaximumLength(256);

        RuleFor(x => x.DimensionsText)
            .MaximumLength(256);

        RuleFor(x => x.EditionSize)
            .MaximumLength(64);

        RuleFor(x => x.EstimatedReleaseText)
            .MaximumLength(128);

        RuleFor(x => x.Status)
            .IsInEnum()
            .Must(status => status != ProductStatus.Archived)
            .WithMessage("Archived status can only be set through archive workflow.");

        RuleForEach(x => x.Specifications)
            .SetValidator(new CreateProductSpecificationInputValidator());
    }
}

public class CreateProductSpecificationInputValidator : AbstractValidator<CreateProductSpecificationInput>
{
    public CreateProductSpecificationInputValidator()
    {
        RuleFor(x => x.Label)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(512);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
