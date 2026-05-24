namespace Ecommerce.Application.Categories.GetCategories;

public record GetCategoriesResponse(IReadOnlyList<CategoryListItemResponse> Items);

public record CategoryListItemResponse(Guid Id, string Name, string Slug, string Description, bool IsActive);
