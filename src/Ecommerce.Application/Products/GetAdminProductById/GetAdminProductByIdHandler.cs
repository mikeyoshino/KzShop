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

        var productProjection = await _context.Products
            .AsNoTracking()
            .Where(product => product.Id == request.Id)
            .Select(product => new
            {
                product.Id,
                product.Name,
                product.Slug,
                product.ShortDescription,
                product.Description,
                product.CategoryId,
                product.StudioId,
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
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (productProjection is null)
        {
            return null;
        }

        var categoryName = await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == productProjection.CategoryId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync(cancellationToken);

        var studioName = await _context.Studios
            .AsNoTracking()
            .Where(x => x.Id == productProjection.StudioId)
            .Select(x => x.Name)
            .SingleOrDefaultAsync(cancellationToken);

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
            categoryName ?? string.Empty,
            productProjection.StudioId,
            studioName ?? string.Empty,
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
