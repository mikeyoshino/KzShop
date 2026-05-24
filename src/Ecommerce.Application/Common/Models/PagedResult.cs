namespace Ecommerce.Application.Common.Models;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);
