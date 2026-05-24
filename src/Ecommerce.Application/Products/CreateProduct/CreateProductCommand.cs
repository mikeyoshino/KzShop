using Ecommerce.Application.Common.Models;
using Ecommerce.Domain.Enums;
using MediatR;

namespace Ecommerce.Application.Products.CreateProduct;

public record CreateProductCommand(
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
    IReadOnlyList<CreateProductSpecificationInput> Specifications) : IRequest<Result<CreateProductResponse>>;

public record CreateProductSpecificationInput(
    string Label,
    string Value,
    int SortOrder);
