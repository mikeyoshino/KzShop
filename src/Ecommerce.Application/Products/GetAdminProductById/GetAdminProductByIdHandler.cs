using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.GetAdminProductById;

public class GetAdminProductByIdHandler : IRequestHandler<GetAdminProductByIdQuery, GetAdminProductByIdResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetAdminProductByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdminProductByIdResponse?> Handle(GetAdminProductByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            return null;
        }

        var productProjection = await (
            from product in _context.Products.AsNoTracking()
            join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id
            join studio in _context.Studios.AsNoTracking() on product.StudioId equals studio.Id
            where product.Id == request.Id
            select new
            {
                product.Id,
                product.Name,
                product.Slug,
                product.ShortDescription,
                product.Description,
                product.CategoryId,
                CategoryName = category.Name,
                product.StudioId,
                StudioName = studio.Name,
                product.Sku,
                product.PriceAmount,
                product.Currency,
                product.Status,
                product.IsNew,
                product.IsFeatured,
                product.Scale,
                product.Materials,
                product.DimensionsText,
                product.EditionSize,
                product.EstimatedReleaseText,
                product.PublishedAt,
                product.ArchivedAt
            }).SingleOrDefaultAsync(cancellationToken);

        if (productProjection is null)
        {
            return null;
        }

        var images = await _context.ProductImages
            .AsNoTracking()
            .Where(x => x.ProductId == request.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new AdminProductImageResponse(x.Id, x.PublicUrl, x.AltText, x.SortOrder))
            .ToListAsync(cancellationToken);

        var specifications = await _context.ProductSpecifications
            .AsNoTracking()
            .Where(x => x.ProductId == request.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new AdminProductSpecificationResponse(x.Id, x.Label, x.Value, x.SortOrder))
            .ToListAsync(cancellationToken);

        var inventory = await _context.InventoryItems
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProductId == request.Id, cancellationToken);

        return new GetAdminProductByIdResponse(
            productProjection.Id,
            productProjection.Name,
            productProjection.Slug,
            productProjection.ShortDescription,
            productProjection.Description,
            productProjection.CategoryId,
            productProjection.CategoryName,
            productProjection.StudioId,
            productProjection.StudioName,
            productProjection.Sku,
            productProjection.PriceAmount,
            productProjection.Currency,
            productProjection.Status.ToString(),
            productProjection.IsNew,
            productProjection.IsFeatured,
            productProjection.Scale,
            productProjection.Materials,
            productProjection.DimensionsText,
            productProjection.EditionSize,
            productProjection.EstimatedReleaseText,
            productProjection.PublishedAt,
            productProjection.ArchivedAt,
            inventory?.OnHandQuantity ?? 0,
            inventory?.ReservedQuantity ?? 0,
            inventory?.AvailableQuantity ?? 0,
            images,
            specifications);
    }
}
