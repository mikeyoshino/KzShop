namespace Ecommerce.Application.Categories.UpdateCategory;

public record UpdateCategoryResponse(Guid Id, string Name, string Slug, string Description, bool IsActive);
