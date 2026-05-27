using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Application.Common.Models;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecommerce.Api.Tests;

public class AdminCatalogEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;
    private readonly TestApiFactory _factory;

    public AdminCatalogEndpointsTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateAdminClientAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CreateStudio_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/studios", new
        {
            name = "XM Studios",
            slug = "xm-studios",
            isActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateStudio_ShouldReturnConflict_WhenDuplicateName()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/studios", new
        {
            name = "Prime Studios",
            slug = "prime-studios-dup",
            isActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var payload = await ReadErrorAsync(response);
        payload.Code.Should().Be((int)BusinessErrorCode.DuplicateName);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk_WithSeededCategory()
    {
        var response = await _client.GetAsync("/api/admin/categories");
        var payload = await response.Content.ReadFromJsonAsync<GetCategoriesContract>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().NotBeNull();
        payload!.Items.Should().Contain(x => x.Slug == "dc-batman");
    }

    [Fact]
    public async Task GetCategories_ShouldFilterBySearch()
    {
        var response = await _client.GetAsync("/api/admin/categories?search=bat");
        var payload = await response.Content.ReadFromJsonAsync<GetCategoriesContract>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().NotBeNull();
        payload!.Items.Should().ContainSingle(x => x.Slug == "dc-batman");
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_AndPersistCategory()
    {
        var slug = $"api-created-{Guid.NewGuid():N}";

        var response = await _client.PostAsJsonAsync("/api/admin/categories", new
        {
            name = "API Created Category",
            slug,
            description = "Created from API integration test",
            isActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var payload = await response.Content.ReadFromJsonAsync<CategoryContract>();
        payload.Should().NotBeNull();
        payload!.Slug.Should().Be(slug);
        payload.Name.Should().Be("API Created Category");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await dbContext.Categories.AsNoTracking().SingleAsync(x => x.Slug == slug);
        persisted.Name.Should().Be("API Created Category");
        persisted.Description.Should().Be("Created from API integration test");
        persisted.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnConflict_WhenDuplicateSlug()
    {
        var response = await _client.PostAsJsonAsync("/api/admin/categories", new
        {
            name = "Duplicate Slug Category",
            slug = "dc-batman",
            description = "Duplicate slug",
            isActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var payload = await ReadErrorAsync(response);
        payload.Code.Should().Be((int)BusinessErrorCode.DuplicateSlug);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnOk_AndPersistCategory()
    {
        var originalSlug = $"api-update-source-{Guid.NewGuid():N}";
        var createResponse = await _client.PostAsJsonAsync("/api/admin/categories", new
        {
            name = "API Update Source",
            slug = originalSlug,
            description = "Before update",
            isActive = true
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryContract>();

        var updatedSlug = $"api-updated-{Guid.NewGuid():N}";
        var response = await _client.PutAsJsonAsync($"/api/admin/categories/{created!.Id:D}", new
        {
            name = "API Updated Category",
            slug = updatedSlug,
            description = "After update",
            isActive = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<CategoryContract>();
        payload.Should().NotBeNull();
        payload!.Id.Should().Be(created.Id);
        payload.Slug.Should().Be(updatedSlug);
        payload.Name.Should().Be("API Updated Category");

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persisted = await dbContext.Categories.AsNoTracking().SingleAsync(x => x.Id == created.Id);
        persisted.Name.Should().Be("API Updated Category");
        persisted.Slug.Should().Be(updatedSlug);
        persisted.Description.Should().Be("After update");
        persisted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        var response = await _client.PutAsJsonAsync($"/api/admin/categories/{Guid.NewGuid():D}", new
        {
            name = "Missing Category",
            slug = "missing-category",
            description = "Missing",
            isActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var payload = await ReadErrorAsync(response);
        payload.Code.Should().Be((int)BusinessErrorCode.CategoryNotFound);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnConflict_WhenDuplicateName()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/admin/categories", new
        {
            name = $"API Duplicate Name Source {Guid.NewGuid():N}",
            slug = $"api-duplicate-name-source-{Guid.NewGuid():N}",
            description = "Before duplicate update",
            isActive = true
        });
        var created = await createResponse.Content.ReadFromJsonAsync<CategoryContract>();

        var response = await _client.PutAsJsonAsync($"/api/admin/categories/{created!.Id:D}", new
        {
            name = "DC / Batman",
            slug = $"api-duplicate-name-target-{Guid.NewGuid():N}",
            description = "Duplicate name",
            isActive = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var payload = await ReadErrorAsync(response);
        payload.Code.Should().Be((int)BusinessErrorCode.DuplicateName);
    }

    [Fact]
    public async Task GetAdminProductById_ShouldReturnOk()
    {
        var productsResponse = await _client.GetFromJsonAsync<GetProductsContract>("/api/admin/products");
        var productId = productsResponse!.Items.First(x => x.Slug == "dark-knight").Id;

        var response = await _client.GetAsync($"/api/admin/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadProductImage_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        using var form = new MultipartFormDataContent();
        using var content = new ByteArrayContent([1, 2, 3]);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        form.Add(content, "file", "sample.jpg");
        form.Add(new StringContent("new alt"), "altText");

        var response = await _client.PostAsync($"/api/admin/products/{Guid.NewGuid():D}/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var payload = await ReadErrorAsync(response);
        payload.Code.Should().Be((int)BusinessErrorCode.ProductNotFound);
    }

    [Fact]
    public async Task UploadProductImage_ShouldReturnBadRequest_WithNumericValidationCode_WhenFileMissing()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("new alt"), "altText");

        var productsResponse = await _client.GetFromJsonAsync<GetProductsContract>("/api/admin/products");
        var productId = productsResponse!.Items.First().Id;

        var response = await _client.PostAsync($"/api/admin/products/{productId:D}/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await ReadErrorAsync(response);
        payload.Code.Should().Be((int)BusinessErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task GetAdminDashboardMetrics_ShouldReturnContractData()
    {
        var response = await _client.GetAsync("/api/admin/dashboard/metrics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var document = await response.Content.ReadFromJsonAsync<JsonElement>();
        document.EnumerateObject().Select(x => x.Name).Should().BeEquivalentTo(
        [
            "periodRevenue",
            "recognizedRevenue",
            "orderCount",
            "criticalInventoryItems",
            "recentOrders"
        ]);

        var payload = JsonSerializer.Deserialize<AdminDashboardMetricsContract>(document.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        payload!.PeriodRevenue.Should().BePositive();
        payload.RecognizedRevenue.Should().BeGreaterThanOrEqualTo(payload.PeriodRevenue);
        payload.OrderCount.Should().Be(5);
        payload.RecentOrders.Should().NotBeEmpty();
        payload.RecentOrders.Should().HaveCountLessThanOrEqualTo(5);
        payload.RecentOrders.Should().BeInDescendingOrder(x => x.CreatedAtUtc);
        payload.CriticalInventoryItems.Should().ContainSingle(x =>
            x.ProductName == "Batman Beyond: Modern Suit" &&
            x.AvailableQuantity < 5);
        payload.CriticalInventoryItems.Should().OnlyContain(x => x.AvailableQuantity < 5);
        payload.CriticalInventoryItems.Should().NotContain(x =>
            x.ProductName == "Nightwing: Bludhaven Vigil" &&
            x.AvailableQuantity >= 5);
    }

    [Fact]
    public async Task TestSeed_ShouldPersistOrders_WithItemsAndStatuses()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var orders = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ToListAsync();

        orders.Should().HaveCount(5);
        orders.Should().OnlyContain(x => x.Items.Count > 0);
        orders.Select(x => x.Status).Should().Contain(OrderStatus.Pending);
        orders.Select(x => x.Status).Should().Contain(OrderStatus.Paid);
        orders.Select(x => x.Status).Should().Contain(OrderStatus.Processing);
        orders.Select(x => x.Status).Should().Contain(OrderStatus.Shipped);
        orders.Select(x => x.Status).Should().Contain(OrderStatus.Cancelled);
    }

    private static async Task<ErrorContract> ReadErrorAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<ErrorContract>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        payload.Should().NotBeNull();
        return payload!;
    }

    private sealed record GetProductsContract(IReadOnlyList<ProductContract> Items);
    private sealed record GetCategoriesContract(IReadOnlyList<CategoryContract> Items);
    private sealed record ErrorContract(int Code, string Message);
    private sealed record AdminDashboardMetricsContract(
        decimal PeriodRevenue,
        decimal RecognizedRevenue,
        int OrderCount,
        IReadOnlyList<RecentOrderContract> RecentOrders,
        IReadOnlyList<CriticalInventoryItemContract> CriticalInventoryItems);

    private sealed record ProductContract(Guid Id, string Slug);
    private sealed record CategoryContract(Guid Id, string Name, string Slug);
    private sealed record RecentOrderContract(Guid OrderId, string OrderNumber, DateTimeOffset CreatedAtUtc);
    private sealed record CriticalInventoryItemContract(Guid ProductId, string ProductName, int AvailableQuantity);
}
