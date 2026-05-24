using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.GetProductBySlug;

public class GetProductBySlugHandler : IRequestHandler<GetProductBySlugQuery, GetProductBySlugResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetProductBySlugHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductBySlugResponse?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return null;
        }

        var slug = request.Slug.Trim().ToLowerInvariant();

        var productProjection = await (
            from product in _context.Products.AsNoTracking()
            join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id
            join studio in _context.Studios.AsNoTracking() on product.StudioId equals studio.Id
            where product.Slug == slug
                && (product.Status == ProductStatus.Active
                    || product.Status == ProductStatus.PreOrder
                    || product.Status == ProductStatus.Waitlist)
            select new
            {
                product.Id,
                product.Name,
                product.Slug,
                StudioName = studio.Name,
                CategorySlug = category.Slug,
                product.PriceAmount,
                product.Currency,
                product.Description,
                product.ShortDescription,
                product.Scale,
                product.Materials,
                product.DimensionsText,
                product.EditionSize,
                product.EstimatedReleaseText,
                product.Status,
                product.IsNew
            }).SingleOrDefaultAsync(cancellationToken);

        if (productProjection is null)
        {
            return null;
        }

        var images = await _context.ProductImages
            .AsNoTracking()
            .Where(x => x.ProductId == productProjection.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new ProductDetailImageResponse(x.Id, x.PublicUrl, x.AltText, x.SortOrder))
            .ToListAsync(cancellationToken);

        var specifications = await _context.ProductSpecifications
            .AsNoTracking()
            .Where(x => x.ProductId == productProjection.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new ProductDetailSpecificationResponse(x.Label, x.Value, x.SortOrder))
            .ToListAsync(cancellationToken);

        return new GetProductBySlugResponse(
            productProjection.Id,
            productProjection.Name,
            productProjection.Slug,
            productProjection.StudioName,
            productProjection.CategorySlug,
            productProjection.PriceAmount,
            productProjection.Currency,
            productProjection.Description,
            productProjection.ShortDescription,
            productProjection.Scale,
            productProjection.Materials,
            productProjection.DimensionsText,
            productProjection.EditionSize,
            productProjection.EstimatedReleaseText,
            productProjection.Status.ToString(),
            productProjection.IsNew,
            images,
            specifications);
    }
}
