using Ecommerce.Domain.Common;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class ProductSpecification : BaseEntity
{
    private ProductSpecification()
    {
    }

    internal ProductSpecification(Guid productId, string label, string value, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new DomainException("Specification label is required.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Specification value is required.");
        }

        if (sortOrder < 0)
        {
            throw new DomainException("Specification sort order cannot be negative.");
        }

        ProductId = productId;
        Label = label.Trim();
        Value = value.Trim();
        SortOrder = sortOrder;
    }

    public Guid ProductId { get; private set; }

    public string Label { get; private set; } = string.Empty;

    public string Value { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }
}
