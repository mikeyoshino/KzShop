using MediatR;

namespace Ecommerce.Application.Products.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IRequest<GetProductBySlugResponse?>;
