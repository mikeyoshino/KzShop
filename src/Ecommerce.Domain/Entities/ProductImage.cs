using Ecommerce.Domain.Common;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class ProductImage : BaseEntity
{
    private ProductImage()
    {
    }

    internal ProductImage(Guid productId, string publicUrl, string altText, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            throw new DomainException("Product image URL is required.");
        }

        if (sortOrder < 0)
        {
            throw new DomainException("Product image sort order cannot be negative.");
        }

        ProductId = productId;
        PublicUrl = publicUrl.Trim();
        AltText = altText?.Trim() ?? string.Empty;
        SortOrder = sortOrder;
    }

    public Guid ProductId { get; private set; }

    public string PublicUrl { get; private set; } = string.Empty;

    public string AltText { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public void UpdateSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new DomainException("Product image sort order cannot be negative.");
        }

        SortOrder = sortOrder;
    }
}
