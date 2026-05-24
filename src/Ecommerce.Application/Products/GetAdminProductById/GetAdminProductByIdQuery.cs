using MediatR;

namespace Ecommerce.Application.Products.GetAdminProductById;

public record GetAdminProductByIdQuery(Guid Id) : IRequest<GetAdminProductByIdResponse?>;
