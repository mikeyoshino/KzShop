using FluentValidation;

namespace Ecommerce.Application.Inventory.AdjustStock;

public class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.QuantityDelta)
            .NotEqual(0);
    }
}
