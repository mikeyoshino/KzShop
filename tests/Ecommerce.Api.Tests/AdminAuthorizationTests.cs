using System.Net;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Api.Tests;

public class AdminAuthorizationTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public AdminAuthorizationTests(TestApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetCategories_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnForbidden_WhenUserIsNotAdmin()
    {
        using var client = await _factory.CreateAuthenticatedClientAsync("customer@example.com", "Customer123!");

        var response = await client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk_WhenUserIsAdmin()
    {
        using var client = await _factory.CreateAdminClientAsync();

        var response = await client.GetAsync("/api/admin/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
