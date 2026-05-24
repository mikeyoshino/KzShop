using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PublicUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.AltText)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.HasIndex(x => new { x.ProductId, x.SortOrder });
    }
}
