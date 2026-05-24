using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProductsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var productsQuery =
            from product in _context.Products.AsNoTracking()
            join category in _context.Categories.AsNoTracking() on product.CategoryId equals category.Id
            join studio in _context.Studios.AsNoTracking() on product.StudioId equals studio.Id
            where product.Status == ProductStatus.Active
                || product.Status == ProductStatus.PreOrder
                || product.Status == ProductStatus.Waitlist
            select new
            {
                Product = product,
                CategorySlug = category.Slug,
                StudioName = studio.Name
            };

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            var categorySlug = request.CategorySlug.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(x => x.CategorySlug == categorySlug);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var statusInput = request.Status.Trim();
            if (!Enum.TryParse<ProductStatus>(statusInput, true, out var parsedStatus))
            {
                return new GetProductsResponse([]);
            }

            productsQuery = productsQuery.Where(x => x.Product.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(request.Scale))
        {
            var scale = request.Scale.Trim().ToLowerInvariant();
            productsQuery = productsQuery.Where(x => x.Product.Scale.ToLower() == scale);
        }

        var projectedQuery =
            from item in productsQuery
            join image in _context.ProductImages.AsNoTracking() on item.Product.Id equals image.ProductId into imageGroup
            select new ProductListItemResponse(
                item.Product.Id,
                item.Product.Name,
                item.Product.Slug,
                item.CategorySlug,
                item.StudioName,
                item.Product.PriceAmount,
                item.Product.Currency,
                item.Product.Scale,
                item.Product.Status.ToString(),
                item.Product.IsNew,
                imageGroup
                    .OrderBy(x => x.SortOrder)
                    .Select(x => x.PublicUrl)
                    .FirstOrDefault());

        var items = await projectedQuery
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return new GetProductsResponse(items);
    }
}
