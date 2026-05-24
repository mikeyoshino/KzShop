using FluentValidation;

namespace Ecommerce.Application.ProductImages.ReorderProductImages;

public class ReorderProductImagesValidator : AbstractValidator<ReorderProductImagesCommand>
{
    public ReorderProductImagesValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.OrderedImageIds)
            .NotNull();

        RuleForEach(x => x.OrderedImageIds)
            .NotEmpty();
    }
}
