using Ecommerce.Domain.Enums;
using MediatR;

namespace Ecommerce.Application.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Slug,
    string ShortDescription,
    string Description,
    Guid CategoryId,
    Guid StudioId,
    string Sku,
    decimal PriceAmount,
    string Currency,
    ProductStatus Status,
    bool IsNew,
    bool IsFeatured,
    string Scale,
    string Materials,
    string DimensionsText,
    string EditionSize,
    string EstimatedReleaseText,
    IReadOnlyList<UpdateProductSpecificationInput> Specifications) : IRequest<UpdateProductResponse>;

public record UpdateProductSpecificationInput(
    string Label,
    string Value,
    int SortOrder);
