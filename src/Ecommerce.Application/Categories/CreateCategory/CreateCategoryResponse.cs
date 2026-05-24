namespace Ecommerce.Application.Categories.CreateCategory;

public record CreateCategoryResponse(Guid Id, string Name, string Slug, string Description, bool IsActive);
