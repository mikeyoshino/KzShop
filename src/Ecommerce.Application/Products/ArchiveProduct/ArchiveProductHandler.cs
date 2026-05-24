using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.ArchiveProduct;

public class ArchiveProductHandler : IRequestHandler<ArchiveProductCommand, ArchiveProductResponse>
{
    private readonly IApplicationDbContext _context;

    public ArchiveProductHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ArchiveProductResponse> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (product is null)
        {
            throw new InvalidOperationException("Product was not found.");
        }

        product.Archive();
        await _context.SaveChangesAsync(cancellationToken);

        return new ArchiveProductResponse(product.Id, product.Status.ToString(), product.ArchivedAt);
    }
}
