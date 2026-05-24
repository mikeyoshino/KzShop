using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Label)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.SortOrder });
    }
}
