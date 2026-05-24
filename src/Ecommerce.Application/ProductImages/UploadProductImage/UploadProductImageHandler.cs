using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.ProductImages.UploadProductImage;

public class UploadProductImageHandler : IRequestHandler<UploadProductImageCommand, Result<UploadProductImageResponse>>
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    private readonly IApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UploadProductImageHandler(IApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<Result<UploadProductImageResponse>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(x => x.Images)
            .SingleOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<UploadProductImageResponse>(BusinessErrorCode.ProductNotFound, "Product was not found.");
        }

        if (!IsValidContentType(request.ContentType))
        {
            return Result.Failure<UploadProductImageResponse>(
                BusinessErrorCode.ValidationFailed,
                "Only JPEG, PNG, WEBP, and GIF image content types are supported.");
        }

        if (!request.Content.CanRead)
        {
            return Result.Failure<UploadProductImageResponse>(
                BusinessErrorCode.ValidationFailed,
                "Image content stream must be readable.");
        }

        var nextSortOrder = product.Images.Count == 0
            ? 0
            : product.Images.Max(x => x.SortOrder) + 1;
        var objectPath = BuildObjectPath(request.ProductId, request.FileName);
        string uploadedPath;

        try
        {
            uploadedPath = await _storageService.UploadAsync(
                objectPath,
                request.Content,
                request.ContentType,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Result.Failure<UploadProductImageResponse>(
                BusinessErrorCode.StorageUploadFailed,
                "Image upload failed.");
        }

        product.AddImage(uploadedPath, request.AltText, nextSortOrder);
        var createdImage = product.Images
            .Single(x => x.SortOrder == nextSortOrder && x.PublicUrl == uploadedPath);
        _context.ProductImages.Add(createdImage);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsPersistenceException(ex))
        {
            try
            {
                await _storageService.DeleteAsync(uploadedPath, cancellationToken);
            }
            catch (Exception compensationEx) when (compensationEx is not OperationCanceledException)
            {
                return Result.Failure<UploadProductImageResponse>(
                    BusinessErrorCode.StorageCompensationFailed,
                    "Image persistence failed and uploaded file cleanup also failed.");
            }

            return Result.Failure<UploadProductImageResponse>(
                BusinessErrorCode.PersistenceFailed,
                "Image persistence failed.");
        }

        return Result.Success(new UploadProductImageResponse(
            createdImage.Id,
            createdImage.ProductId,
            createdImage.PublicUrl,
            createdImage.AltText,
            createdImage.SortOrder));
    }

    private static bool IsValidContentType(string contentType)
    {
        return AllowedContentTypes.Contains(contentType.Trim());
    }

    private static string BuildObjectPath(Guid productId, string fileName)
    {
        var safeFileName = Path.GetFileName(fileName).Trim();
        var sanitizedFileName = string.Join(
            "-",
            safeFileName
                .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            sanitizedFileName = "image";
        }

        return $"products/{productId:D}/{Guid.NewGuid():N}-{sanitizedFileName}";
    }

    private static bool IsPersistenceException(Exception ex)
    {
        return ex is DbUpdateException or DbUpdateConcurrencyException or TimeoutException;
    }
}
