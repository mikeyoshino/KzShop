using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.ProductImages.ReorderProductImages;

public class ReorderProductImagesHandler : IRequestHandler<ReorderProductImagesCommand, Result<ReorderProductImagesResponse>>
{
    private readonly IApplicationDbContext _context;

    public ReorderProductImagesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ReorderProductImagesResponse>> Handle(ReorderProductImagesCommand request, CancellationToken cancellationToken)
    {
        var hasProduct = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductId, cancellationToken);
        if (!hasProduct)
        {
            return Result.Failure<ReorderProductImagesResponse>(BusinessErrorCode.ProductNotFound, "Product was not found.");
        }

        var images = await _context.ProductImages
            .Where(x => x.ProductId == request.ProductId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (images.Count != request.OrderedImageIds.Count)
        {
            return Result.Failure<ReorderProductImagesResponse>(
                BusinessErrorCode.ValidationFailed,
                "Reorder request must include every image exactly once.");
        }

        if (request.OrderedImageIds.Distinct().Count() != request.OrderedImageIds.Count)
        {
            return Result.Failure<ReorderProductImagesResponse>(
                BusinessErrorCode.ValidationFailed,
                "Reorder request contains duplicate image IDs.");
        }

        var imagesById = images.ToDictionary(x => x.Id);
        if (request.OrderedImageIds.Any(id => !imagesById.ContainsKey(id)))
        {
            return Result.Failure<ReorderProductImagesResponse>(
                BusinessErrorCode.ValidationFailed,
                "Reorder request contains image IDs not associated with this product.");
        }

        for (var index = 0; index < request.OrderedImageIds.Count; index++)
        {
            var image = imagesById[request.OrderedImageIds[index]];
            image.UpdateSortOrder(index);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var normalizedImages = images
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => new ReorderedProductImageResponse(x.Id, x.SortOrder))
            .ToList();

        return Result.Success(new ReorderProductImagesResponse(request.ProductId, normalizedImages));
    }
}
