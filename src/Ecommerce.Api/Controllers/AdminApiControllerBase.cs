using Ecommerce.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
public abstract class AdminApiControllerBase : ControllerBase
{
    protected ActionResult MapFailure(Result result)
    {
        var payload = new { code = result.ErrorCode?.ToString(), message = result.ErrorMessage };

        return result.ErrorCode switch
        {
            BusinessErrorCode.ValidationFailed => BadRequest(payload),
            BusinessErrorCode.CategoryNotFound => NotFound(payload),
            BusinessErrorCode.StudioNotFound => NotFound(payload),
            BusinessErrorCode.ProductNotFound => NotFound(payload),
            BusinessErrorCode.InventoryNotFound => NotFound(payload),
            BusinessErrorCode.DuplicateName => Conflict(payload),
            BusinessErrorCode.DuplicateSlug => Conflict(payload),
            BusinessErrorCode.DuplicateSku => Conflict(payload),
            BusinessErrorCode.InvalidStatusTransition => Conflict(payload),
            BusinessErrorCode.StorageUploadFailed => StatusCode(StatusCodes.Status502BadGateway, payload),
            BusinessErrorCode.StorageDeleteFailed => StatusCode(StatusCodes.Status502BadGateway, payload),
            BusinessErrorCode.StorageCompensationFailed => StatusCode(StatusCodes.Status500InternalServerError, payload),
            BusinessErrorCode.PersistenceFailed => StatusCode(StatusCodes.Status500InternalServerError, payload),
            _ => BadRequest(payload)
        };
    }
}
