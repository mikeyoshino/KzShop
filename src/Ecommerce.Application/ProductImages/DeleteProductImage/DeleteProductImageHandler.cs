using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.ProductImages.DeleteProductImage;

public class DeleteProductImageHandler : IRequestHandler<DeleteProductImageCommand, Result<DeleteProductImageResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public DeleteProductImageHandler(IApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<Result<DeleteProductImageResponse>> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var hasProduct = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductId, cancellationToken);
        if (!hasProduct)
        {
            return Result.Failure<DeleteProductImageResponse>(BusinessErrorCode.ProductNotFound, "Product was not found.");
        }

        var images = await _context.ProductImages
            .Where(x => x.ProductId == request.ProductId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var imageToDelete = images.SingleOrDefault(x => x.Id == request.ProductImageId);
        if (imageToDelete is null)
        {
            return Result.Failure<DeleteProductImageResponse>(
                BusinessErrorCode.ValidationFailed,
                "Image was not found for this product.");
        }

        _context.ProductImages.Remove(imageToDelete);

        var remainingImages = images
            .Where(x => x.Id != request.ProductImageId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToList();

        for (var index = 0; index < remainingImages.Count; index++)
        {
            remainingImages[index].UpdateSortOrder(index);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsPersistenceException(ex))
        {
            return Result.Failure<DeleteProductImageResponse>(
                BusinessErrorCode.PersistenceFailed,
                "Image delete persistence failed.");
        }

        try
        {
            await _storageService.DeleteAsync(imageToDelete.PublicUrl, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<DeleteProductImageResponse>(
                BusinessErrorCode.StorageDeleteFailed,
                "Image deleted from database but storage cleanup failed.");
        }

        return Result.Success(new DeleteProductImageResponse(
            request.ProductId,
            request.ProductImageId,
            remainingImages.Count));
    }

    private static bool IsPersistenceException(Exception ex)
    {
        return ex is DbUpdateException or DbUpdateConcurrencyException or TimeoutException;
    }
}
