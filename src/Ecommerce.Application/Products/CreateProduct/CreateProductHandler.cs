using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateProductHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedSku = request.Sku.Trim();

        var hasDuplicateSlug = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            return Result.Failure<CreateProductResponse>(BusinessErrorCode.DuplicateSlug, "Product slug already exists.");
        }

        var hasDuplicateSku = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Sku == normalizedSku, cancellationToken);
        if (hasDuplicateSku)
        {
            return Result.Failure<CreateProductResponse>(BusinessErrorCode.DuplicateSku, "Product SKU already exists.");
        }

        var hasCategory = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!hasCategory)
        {
            return Result.Failure<CreateProductResponse>(BusinessErrorCode.CategoryNotFound, "Category was not found.");
        }

        var hasStudio = await _context.Studios
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.StudioId, cancellationToken);
        if (!hasStudio)
        {
            return Result.Failure<CreateProductResponse>(BusinessErrorCode.StudioNotFound, "Studio was not found.");
        }

        var product = Product.Create(
            request.Name,
            request.Slug,
            request.ShortDescription,
            request.Description,
            request.CategoryId,
            request.StudioId,
            request.Sku,
            request.PriceAmount,
            request.Currency,
            request.Status,
            request.IsNew,
            request.IsFeatured,
            request.Scale,
            request.Materials,
            request.DimensionsText,
            request.EditionSize,
            request.EstimatedReleaseText);

        foreach (var specification in request.Specifications.OrderBy(x => x.SortOrder))
        {
            product.AddSpecification(specification.Label, specification.Value, specification.SortOrder);
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateProductResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Sku,
            product.Status.ToString()));
    }
}
