namespace Ecommerce.Application.Common.Models;

public enum BusinessErrorCode
{
    DuplicateSlug,
    DuplicateName,
    DuplicateSku,
    CategoryNotFound,
    StudioNotFound,
    ProductNotFound,
    InventoryNotFound,
    InvalidStatusTransition,
    StorageUploadFailed,
    StorageDeleteFailed,
    ValidationFailed
}
