using FluentValidation;

namespace Ecommerce.Application.ProductImages.UploadProductImage;

public class UploadProductImageValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(260);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Content)
            .NotNull();

        RuleFor(x => x.AltText)
            .MaximumLength(256);
    }
}
