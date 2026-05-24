namespace Ecommerce.Application.Products.ArchiveProduct;

public record ArchiveProductResponse(
    Guid Id,
    string Status,
    DateTimeOffset? ArchivedAt);
