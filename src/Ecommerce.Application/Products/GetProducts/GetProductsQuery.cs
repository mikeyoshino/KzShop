using MediatR;

namespace Ecommerce.Application.Products.GetProducts;

public record GetProductsQuery(string? CategorySlug, string? Status, string? Scale) : IRequest<GetProductsResponse>;
