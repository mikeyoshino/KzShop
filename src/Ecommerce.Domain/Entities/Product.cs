using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class Product : BaseEntity
{
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductSpecification> _specifications = [];

    private Product()
    {
    }

    public Guid CategoryId { get; private set; }

    public Guid StudioId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public string ShortDescription { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public decimal PriceAmount { get; private set; }

    public string Currency { get; private set; } = "USD";

    public ProductStatus Status { get; private set; }

    public bool IsNew { get; private set; }

    public bool IsFeatured { get; private set; }

    public string Scale { get; private set; } = string.Empty;

    public string Materials { get; private set; } = string.Empty;

    public string DimensionsText { get; private set; } = string.Empty;

    public string EditionSize { get; private set; } = string.Empty;

    public string EstimatedReleaseText { get; private set; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

    public IReadOnlyCollection<ProductImage> Images => _images;

    public IReadOnlyCollection<ProductSpecification> Specifications => _specifications;

    public bool IsPurchasable => Status is ProductStatus.Active or ProductStatus.PreOrder;

    public static Product Create(
        string name,
        string slug,
        string shortDescription,
        string description,
        Guid categoryId,
        Guid studioId,
        string sku,
        decimal priceAmount,
        string currency,
        ProductStatus status,
        bool isNew,
        bool isFeatured,
        string scale,
        string materials,
        string dimensionsText,
        string editionSize,
        string estimatedReleaseText)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Product slug is required.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("Product SKU is required.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category is required.");
        }

        if (studioId == Guid.Empty)
        {
            throw new DomainException("Studio is required.");
        }

        if (priceAmount <= 0m)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            ShortDescription = shortDescription?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? string.Empty,
            CategoryId = categoryId,
            StudioId = studioId,
            Sku = sku.Trim(),
            PriceAmount = priceAmount,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = status,
            IsNew = isNew,
            IsFeatured = isFeatured,
            Scale = scale?.Trim() ?? string.Empty,
            Materials = materials?.Trim() ?? string.Empty,
            DimensionsText = dimensionsText?.Trim() ?? string.Empty,
            EditionSize = editionSize?.Trim() ?? string.Empty,
            EstimatedReleaseText = estimatedReleaseText?.Trim() ?? string.Empty,
            PublishedAt = status == ProductStatus.Active ? DateTimeOffset.UtcNow : null
        };
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        ArchivedAt = DateTimeOffset.UtcNow;
    }

    public void Update(
        string name,
        string slug,
        string shortDescription,
        string description,
        Guid categoryId,
        Guid studioId,
        string sku,
        decimal priceAmount,
        string currency,
        ProductStatus status,
        bool isNew,
        bool isFeatured,
        string scale,
        string materials,
        string dimensionsText,
        string editionSize,
        string estimatedReleaseText)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new DomainException("Product slug is required.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("Product SKU is required.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category is required.");
        }

        if (studioId == Guid.Empty)
        {
            throw new DomainException("Studio is required.");
        }

        if (priceAmount <= 0m)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ShortDescription = shortDescription?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
        CategoryId = categoryId;
        StudioId = studioId;
        Sku = sku.Trim();
        PriceAmount = priceAmount;
        Currency = currency.Trim().ToUpperInvariant();
        Status = status;
        IsNew = isNew;
        IsFeatured = isFeatured;
        Scale = scale?.Trim() ?? string.Empty;
        Materials = materials?.Trim() ?? string.Empty;
        DimensionsText = dimensionsText?.Trim() ?? string.Empty;
        EditionSize = editionSize?.Trim() ?? string.Empty;
        EstimatedReleaseText = estimatedReleaseText?.Trim() ?? string.Empty;

        if (status == ProductStatus.Active && PublishedAt is null)
        {
            PublishedAt = DateTimeOffset.UtcNow;
        }
    }

    public void AddImage(string publicUrl, string altText, int sortOrder)
    {
        _images.Add(new ProductImage(Id, publicUrl, altText, sortOrder));
    }

    public void AddSpecification(string label, string value, int sortOrder)
    {
        _specifications.Add(new ProductSpecification(Id, label, value, sortOrder));
    }

    public void ReplaceSpecifications(IEnumerable<(string Label, string Value, int SortOrder)> specifications)
    {
        _specifications.Clear();

        foreach (var specification in specifications.OrderBy(x => x.SortOrder))
        {
            _specifications.Add(new ProductSpecification(Id, specification.Label, specification.Value, specification.SortOrder));
        }
    }
}
