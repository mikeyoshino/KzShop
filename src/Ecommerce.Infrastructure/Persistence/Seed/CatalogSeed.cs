using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Seed;

public static class CatalogSeed
{
    public static async Task SeedDevelopmentCatalogAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingProductSlugs = await dbContext.Products
            .Select(x => x.Slug)
            .ToHashSetAsync(cancellationToken);

        var hasSeedProducts =
            existingProductSlugs.Contains("dark-knight")
            && existingProductSlugs.Contains("arkham-joker")
            && existingProductSlugs.Contains("berserker-guts");

        if (!hasSeedProducts)
        {
            var categoriesBySlug = await dbContext.Categories
                .ToDictionaryAsync(x => x.Slug, cancellationToken);
            var studiosBySlug = await dbContext.Studios
                .ToDictionaryAsync(x => x.Slug, cancellationToken);

            if (!categoriesBySlug.TryGetValue("dc-batman", out var dcBatmanCategory))
            {
                dcBatmanCategory = new Category("DC / Batman", "dc-batman", "DC licensed Batman collectibles.", true);
                dbContext.Categories.Add(dcBatmanCategory);
                categoriesBySlug[dcBatmanCategory.Slug] = dcBatmanCategory;
            }

            if (!categoriesBySlug.TryGetValue("anime", out var animeCategory))
            {
                animeCategory = new Category("Anime", "anime", "Premium anime figure collection.", true);
                dbContext.Categories.Add(animeCategory);
                categoriesBySlug[animeCategory.Slug] = animeCategory;
            }

            if (!studiosBySlug.TryGetValue("prime-studios", out var primeStudios))
            {
                primeStudios = new Studio("Prime Studios", "prime-studios", true);
                dbContext.Studios.Add(primeStudios);
                studiosBySlug[primeStudios.Slug] = primeStudios;
            }

            if (!studiosBySlug.TryGetValue("hot-toys", out var hotToys))
            {
                hotToys = new Studio("Hot Toys", "hot-toys", true);
                dbContext.Studios.Add(hotToys);
                studiosBySlug[hotToys.Slug] = hotToys;
            }

            if (!studiosBySlug.TryGetValue("dark-art-studios", out var darkArtStudios))
            {
                darkArtStudios = new Studio("Dark Art Studios", "dark-art-studios", true);
                dbContext.Studios.Add(darkArtStudios);
                studiosBySlug[darkArtStudios.Slug] = darkArtStudios;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            existingProductSlugs = await dbContext.Products
                .Select(x => x.Slug)
                .ToHashSetAsync(cancellationToken);

            if (!existingProductSlugs.Contains("dark-knight"))
            {
                var darkKnight = Product.Create(
                    name: "The Dark Knight: Premium Format",
                    slug: "dark-knight",
                    shortDescription: "1/4 scale cinematic masterpiece with tailored suit and display base.",
                    description: "A cinematic masterpiece capturing the brooding essence of Gotham's protector.",
                    categoryId: dcBatmanCategory.Id,
                    studioId: primeStudios.Id,
                    sku: "PRIME-DK-14-0001",
                    priceAmount: 650.00m,
                    currency: "USD",
                    status: ProductStatus.PreOrder,
                    isNew: true,
                    isFeatured: true,
                    scale: "1/4 Scale",
                    materials: "Polystone, Fabric, PVC",
                    dimensionsText: "H: 24 x W: 12 x D: 10",
                    editionSize: "1000",
                    estimatedReleaseText: "Est. Q4 2026");

                darkKnight.AddImage("https://cdn.toyshops.local/dark-knight/hero.jpg", "Dark Knight 1/4", 0);
                darkKnight.AddImage("https://cdn.toyshops.local/dark-knight/detail.jpg", "Detail Shot", 1);
                darkKnight.AddImage("https://cdn.toyshops.local/dark-knight/base.jpg", "Base Details", 2);
                darkKnight.AddSpecification("Franchise", "DC Comics", 0);
                darkKnight.AddSpecification("Scale", "1/4 Scale", 1);
                darkKnight.AddSpecification("Materials", "Polystone, Fabric, PVC", 2);
                darkKnight.AddSpecification("Est. Release", "Q4 2026", 3);
                dbContext.Products.Add(darkKnight);
                dbContext.InventoryItems.Add(new InventoryItem(darkKnight.Id, 0));
            }

            if (!existingProductSlugs.Contains("arkham-joker"))
            {
                var arkhamJoker = Product.Create(
                    name: "Joker: Arkham Asylum",
                    slug: "arkham-joker",
                    shortDescription: "1/6 scale Joker collectible from Arkham Asylum.",
                    description: "Iconic Arkham interpretation with museum-grade paint application.",
                    categoryId: dcBatmanCategory.Id,
                    studioId: hotToys.Id,
                    sku: "HOT-ARK-JOK-0001",
                    priceAmount: 320.00m,
                    currency: "USD",
                    status: ProductStatus.Active,
                    isNew: false,
                    isFeatured: false,
                    scale: "1/6 Scale",
                    materials: "PVC, Fabric",
                    dimensionsText: "H: 12 x W: 6 x D: 5",
                    editionSize: "Open",
                    estimatedReleaseText: "In Stock");

                arkhamJoker.AddImage("https://cdn.toyshops.local/arkham-joker/hero.jpg", "Arkham Joker", 0);
                arkhamJoker.AddSpecification("Franchise", "DC Comics", 0);
                arkhamJoker.AddSpecification("Scale", "1/6 Scale", 1);
                arkhamJoker.AddSpecification("Materials", "PVC, Fabric", 2);
                dbContext.Products.Add(arkhamJoker);
                dbContext.InventoryItems.Add(new InventoryItem(arkhamJoker.Id, 24));
            }

            if (!existingProductSlugs.Contains("berserker-guts"))
            {
                var berserkerGuts = Product.Create(
                    name: "Berserker Armor Guts",
                    slug: "berserker-guts",
                    shortDescription: "Massive 1/3 scale Berserk centerpiece.",
                    description: "A dramatic interpretation of Guts in the Berserker armor with battle-worn finish.",
                    categoryId: animeCategory.Id,
                    studioId: darkArtStudios.Id,
                    sku: "DARKART-BERS-GUTS-0001",
                    priceAmount: 1200.00m,
                    currency: "USD",
                    status: ProductStatus.Waitlist,
                    isNew: false,
                    isFeatured: true,
                    scale: "1/3 Scale",
                    materials: "Polystone, Resin",
                    dimensionsText: "H: 30 x W: 18 x D: 14",
                    editionSize: "500",
                    estimatedReleaseText: "Waitlist");

                berserkerGuts.AddImage("https://cdn.toyshops.local/berserker-guts/hero.jpg", "Guts Berserker Armor", 0);
                berserkerGuts.AddSpecification("Franchise", "Berserk", 0);
                berserkerGuts.AddSpecification("Scale", "1/3 Scale", 1);
                berserkerGuts.AddSpecification("Materials", "Polystone, Resin", 2);
                dbContext.Products.Add(berserkerGuts);
                dbContext.InventoryItems.Add(new InventoryItem(berserkerGuts.Id, 0));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (await dbContext.Orders.AnyAsync(cancellationToken))
        {
            return;
        }

        var productsBySlug = await dbContext.Products
            .ToDictionaryAsync(x => x.Slug, cancellationToken);

        var dashboardOrderInputs = new[]
        {
            new { Slug = "dark-knight", Quantity = 1, Status = OrderStatus.Paid },
            new { Slug = "arkham-joker", Quantity = 1, Status = OrderStatus.Cancelled },
            new { Slug = "berserker-guts", Quantity = 1, Status = OrderStatus.Pending },
            new { Slug = "arkham-joker", Quantity = 1, Status = OrderStatus.Processing },
            new { Slug = "dark-knight", Quantity = 1, Status = OrderStatus.Shipped }
        };

        foreach (var orderInput in dashboardOrderInputs)
        {
            var product = productsBySlug[orderInput.Slug];
            var order = Order.Create(
                $"ORD-DEV-{Guid.NewGuid():N}"[..16],
                "Dashboard Seed",
                "dashboard-seed@toyshops.local",
                DateTimeOffset.UtcNow,
                [(product.Id, product.Name, product.Sku, orderInput.Quantity, product.PriceAmount, product.Currency)],
                orderInput.Status);
            dbContext.Orders.Add(order);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
