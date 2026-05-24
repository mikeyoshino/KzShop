using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();
        var normalizedSku = request.Sku.Trim();

        var product = await _context.Products
            .Include(x => x.Specifications)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (product is null)
        {
            return Result.Failure<UpdateProductResponse>(BusinessErrorCode.ProductNotFound, "Product was not found.");
        }

        var hasDuplicateSlug = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Slug == normalizedSlug, cancellationToken);
        if (hasDuplicateSlug)
        {
            return Result.Failure<UpdateProductResponse>(BusinessErrorCode.DuplicateSlug, "Product slug already exists.");
        }

        var hasDuplicateSku = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id != request.Id && x.Sku == normalizedSku, cancellationToken);
        if (hasDuplicateSku)
        {
            return Result.Failure<UpdateProductResponse>(BusinessErrorCode.DuplicateSku, "Product SKU already exists.");
        }

        var hasCategory = await _context.Categories
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!hasCategory)
        {
            return Result.Failure<UpdateProductResponse>(BusinessErrorCode.CategoryNotFound, "Category was not found.");
        }

        var hasStudio = await _context.Studios
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.StudioId, cancellationToken);
        if (!hasStudio)
        {
            return Result.Failure<UpdateProductResponse>(BusinessErrorCode.StudioNotFound, "Studio was not found.");
        }

        product.Update(
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

        var existingSpecifications = product.Specifications.ToList();
        if (existingSpecifications.Count > 0)
        {
            _context.ProductSpecifications.RemoveRange(existingSpecifications);
        }

        product.ReplaceSpecifications(
            request.Specifications.Select(x => (x.Label, x.Value, x.SortOrder)));
        _context.ProductSpecifications.AddRange(product.Specifications);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateProductResponse(
            product.Id,
            product.Name,
            product.Slug,
            product.Sku,
            product.Status.ToString()));
    }
}
