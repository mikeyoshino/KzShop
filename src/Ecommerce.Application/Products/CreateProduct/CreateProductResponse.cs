namespace Ecommerce.Application.Products.CreateProduct;

public record CreateProductResponse(
    Guid Id,
    string Name,
    string Slug,
    string Sku,
    string Status);
