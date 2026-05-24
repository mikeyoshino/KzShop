using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Api.Tests;

public class StorefrontCatalogEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public StorefrontCatalogEndpointsTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/storefront/products");
        var payload = await response.Content.ReadFromJsonAsync<GetProductsContract>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().NotBeNull();
        payload!.Items.Should().NotBeEmpty();
        payload.Items.Should().Contain(x => x.Slug == "dark-knight");
    }

    [Fact]
    public async Task GetProductBySlug_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/storefront/products/dark-knight");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductBySlug_ShouldReturnNotFound_ForUnknownSlug()
    {
        var response = await _client.GetAsync("/api/storefront/products/unknown-slug");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record GetProductsContract(IReadOnlyList<ProductContract> Items);

    private sealed record ProductContract(string Slug);
}
