using MediatR;

namespace Ecommerce.Application.Categories.GetCategories;

public record GetCategoriesQuery(string? Search) : IRequest<GetCategoriesResponse>;
