using FluentValidation;

namespace Ecommerce.Application.ProductImages.DeleteProductImage;

public class DeleteProductImageValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.ProductImageId)
            .NotEmpty();
    }
}
