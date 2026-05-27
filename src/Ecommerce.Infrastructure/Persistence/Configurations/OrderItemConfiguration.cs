using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property<Guid>("Id");
        builder.HasKey("Id");

        builder.Property<Guid>("OrderId")
            .IsRequired();

        builder.Property(x => x.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ProductSku)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPriceAmount)
            .HasColumnType("numeric(18,2)");

        builder.Property(x => x.Currency)
            .HasMaxLength(8)
            .IsRequired();

        builder.HasIndex(x => x.ProductId);
    }
}
