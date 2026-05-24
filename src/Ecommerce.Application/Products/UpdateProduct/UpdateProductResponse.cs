namespace Ecommerce.Application.Products.UpdateProduct;

public record UpdateProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    string Status);
