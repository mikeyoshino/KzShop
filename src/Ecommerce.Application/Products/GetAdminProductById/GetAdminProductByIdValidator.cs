using FluentValidation;

namespace Ecommerce.Application.Products.GetAdminProductById;

public class GetAdminProductByIdValidator : AbstractValidator<GetAdminProductByIdQuery>
{
    public GetAdminProductByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product id is required.");
    }
}
