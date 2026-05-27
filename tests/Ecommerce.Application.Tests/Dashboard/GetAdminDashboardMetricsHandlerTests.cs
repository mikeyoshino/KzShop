using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Dashboard.GetAdminDashboardMetrics;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Dashboard;

public class GetAdminDashboardMetricsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCalculateRevenueAndOrderCount_FromPersistedOrders()
    {
        await using var context = DashboardTestApplicationDbContextFactory.Create();
        var handler = new GetAdminDashboardMetricsHandler(context);

        var response = await handler.Handle(new GetAdminDashboardMetricsQuery(), CancellationToken.None);

        response.PeriodRevenue.Should().Be(710m);
        response.RecognizedRevenue.Should().Be(1110m);
        response.OrderCount.Should().Be(7);
    }

    [Fact]
    public async Task Handle_ShouldReturnCriticalInventory_ForActiveProductsOnly()
    {
        await using var context = DashboardTestApplicationDbContextFactory.Create();
        var handler = new GetAdminDashboardMetricsHandler(context);

        var response = await handler.Handle(new GetAdminDashboardMetricsQuery(), CancellationToken.None);

        response.CriticalInventoryItems.Should().HaveCount(1);
        response.CriticalInventoryItems[0].ProductName.Should().Be("Low Stock Active");
        response.CriticalInventoryItems[0].AvailableQuantity.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnMostRecentFiveOrders_SortedDescendingByCreatedAtUtc()
    {
        await using var context = DashboardTestApplicationDbContextFactory.Create();
        var handler = new GetAdminDashboardMetricsHandler(context);

        var response = await handler.Handle(new GetAdminDashboardMetricsQuery(), CancellationToken.None);

        response.RecentOrders.Select(x => x.OrderNumber).Should().Equal(
            "ORD-1006",
            "ORD-1005",
            "ORD-1004",
            "ORD-1003",
            "ORD-1002");

        response.RecentOrders.Should().HaveCount(5);
    }
}

internal static class DashboardTestApplicationDbContextFactory
{
    public static DashboardTestApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<DashboardTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new DashboardTestApplicationDbContext(options);

        var category = new Category("Figures", "figures", "Figures", true);
        var studio = new Studio("Prime Studios", "prime-studios", true);

        var lowStockActive = CreateProduct("Low Stock Active", "low-stock-active", "SKU-LOW-001", ProductStatus.Active, category.Id, studio.Id);
        var lowStockArchived = CreateProduct("Low Stock Archived", "low-stock-archived", "SKU-LOW-002", ProductStatus.Archived, category.Id, studio.Id);
        var healthyActive = CreateProduct("Healthy Active", "healthy-active", "SKU-HGH-003", ProductStatus.Active, category.Id, studio.Id);
        var lowStockActiveInventory = new InventoryItem(lowStockActive.Id, 4);
        lowStockActiveInventory.Reserve(1);

        context.Categories.Add(category);
        context.Studios.Add(studio);
        context.Products.AddRange(lowStockActive, lowStockArchived, healthyActive);
        context.InventoryItems.AddRange(
            lowStockActiveInventory,
            new InventoryItem(lowStockArchived.Id, 4),
            new InventoryItem(healthyActive.Id, 10));

        var now = DateTimeOffset.UtcNow;
        context.Orders.AddRange(
            CreateOrder("ORD-1001", "Bruce Wayne", now.AddDays(-10), 110m, OrderStatus.Paid),
            CreateOrder("ORD-1002", "Clark Kent", now.AddDays(-8), 200m, OrderStatus.Pending),
            CreateOrder("ORD-1003", "Diana Prince", now.AddDays(-7), 90m, OrderStatus.Processing),
            CreateOrder("ORD-1004", "Barry Allen", now.AddDays(-4), 210m, OrderStatus.Shipped),
            CreateOrder("ORD-1005", "Hal Jordan", now.AddDays(-2), 50m, OrderStatus.Cancelled),
            CreateOrder("ORD-1006", "Arthur Curry", now.AddDays(-1), 300m, OrderStatus.Paid),
            CreateOrder("ORD-1007", "Victor Stone", now.AddDays(-31), 400m, OrderStatus.Paid));

        context.SaveChanges();
        context.ChangeTracker.Clear();

        return context;
    }

    private static Product CreateProduct(
        string name,
        string slug,
        string sku,
        ProductStatus status,
        Guid categoryId,
        Guid studioId)
    {
        return Product.Create(
            name,
            slug,
            "Short",
            "Long",
            categoryId,
            studioId,
            sku,
            100m,
            "USD",
            status,
            false,
            false,
            "1/6 Scale",
            "PVC",
            "H: 12",
            "Standard",
            "Available");
    }

    private static Order CreateOrder(string orderNumber, string customerName, DateTimeOffset createdAtUtc, decimal totalAmount, OrderStatus status)
    {
        return Order.Create(
            orderNumber,
            customerName,
            $"{orderNumber.ToLowerInvariant()}@example.com",
            createdAtUtc,
            [(
                Guid.NewGuid(),
                "Product",
                $"{orderNumber}-SKU",
                1,
                totalAmount,
                "USD")],
            status);
    }
}

internal sealed class DashboardTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public DashboardTestApplicationDbContext(DbContextOptions<DashboardTestApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderItem>().Property<Guid>("Id");
        modelBuilder.Entity<OrderItem>().HasKey("Id");
    }
}
