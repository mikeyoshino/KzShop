using MediatR;

namespace Ecommerce.Application.Categories.CreateCategory;

public record CreateCategoryCommand(string Name, string Slug, string Description, bool IsActive) : IRequest<CreateCategoryResponse>;
