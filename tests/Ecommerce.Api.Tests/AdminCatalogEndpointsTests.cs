using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Api.Tests;

public class AdminCatalogEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public AdminCatalogEndpointsTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
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
    }

    private sealed record GetProductsContract(IReadOnlyList<ProductContract> Items);

    private sealed record ProductContract(Guid Id, string Slug);
}
