using MediatR;

namespace Ecommerce.Application.Products.ArchiveProduct;

public record ArchiveProductCommand(Guid Id) : IRequest<ArchiveProductResponse>;
