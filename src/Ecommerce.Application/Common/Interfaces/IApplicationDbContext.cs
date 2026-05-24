using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Studio> Studios { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductSpecification> ProductSpecifications { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
