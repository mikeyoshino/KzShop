namespace Ecommerce.Application.Studios.SearchStudios;

public record SearchStudiosResponse(IReadOnlyList<StudioSearchItemResponse> Items);

public record StudioSearchItemResponse(Guid Id, string Name, string Slug, bool IsActive);
