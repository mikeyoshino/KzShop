using Ecommerce.Application.Common.Models;
using MediatR;

namespace Ecommerce.Application.Categories.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name, string Slug, string Description, bool IsActive) : IRequest<Result<UpdateCategoryResponse>>;
