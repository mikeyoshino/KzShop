using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecommerce.Api.Tests;

public class AuthEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;
    private readonly TestApiFactory _factory;

    public AuthEndpointsTests(TestApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnAccessToken_WithAdminRoleClaim()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@example.com",
            password = "Admin123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseContract>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(payload.AccessToken);
        jwt.Claims.Should().Contain(x => x.Type == "user_role" && x.Value == "admin");
    }

    [Fact]
    public async Task Login_ShouldSetHttpOnlyRefreshCookie()
    {
        var response = await LoginAsAdminAsync(_client);

        response.Headers.TryGetValues("Set-Cookie", out var values).Should().BeTrue();
        var refreshCookie = values!.Single(x => x.StartsWith("toyshops_refresh=", StringComparison.OrdinalIgnoreCase));
        refreshCookie.Should().Contain("httponly");
        refreshCookie.Should().Contain("path=/api/auth");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@example.com",
            password = "wrong-password"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldRotateRefreshToken()
    {
        await ClearRefreshTokensAsync();
        await LoginAsAdminAsync(_client);

        var response = await _client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseContract>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = await dbContext.Set<RefreshToken>().AsNoTracking().ToListAsync();
        tokens.Should().HaveCount(2);
        tokens.Should().ContainSingle(x => x.RevokedAtUtc != null && x.ReplacedByTokenId != null);
        tokens.Should().ContainSingle(x => x.RevokedAtUtc == null);
    }

    [Fact]
    public async Task Refresh_ShouldRejectReusedRefreshToken()
    {
        await ClearRefreshTokensAsync();
        var loginResponse = await LoginAsAdminAsync(_client);
        var originalCookie = GetRefreshCookieHeader(loginResponse);

        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var reuseClient = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = false
        });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        request.Headers.Add("Cookie", originalCookie);

        var reuseResponse = await reuseClient.SendAsync(request);

        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken_AndClearCookie()
    {
        await ClearRefreshTokensAsync();
        await LoginAsAdminAsync(_client);

        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.TryGetValues("Set-Cookie", out var values).Should().BeTrue();
        values!.Should().Contain(x =>
            x.StartsWith("toyshops_refresh=", StringComparison.OrdinalIgnoreCase) &&
            x.Contains("expires=", StringComparison.OrdinalIgnoreCase));

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var token = await dbContext.Set<RefreshToken>().AsNoTracking().SingleAsync();
        token.RevokedAtUtc.Should().NotBeNull();

        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static Task<HttpResponseMessage> LoginAsAdminAsync(HttpClient client)
    {
        return client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@example.com",
            password = "Admin123!"
        });
    }

    private static string GetRefreshCookieHeader(HttpResponseMessage response)
    {
        response.Headers.TryGetValues("Set-Cookie", out var values).Should().BeTrue();
        var setCookie = values!.Single(x => x.StartsWith("toyshops_refresh=", StringComparison.OrdinalIgnoreCase));
        return setCookie.Split(';')[0];
    }

    private async Task ClearRefreshTokensAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Set<RefreshToken>().RemoveRange(dbContext.Set<RefreshToken>());
        await dbContext.SaveChangesAsync();
    }

    private sealed record AuthResponseContract(string AccessToken, DateTimeOffset ExpiresAtUtc, AuthUserContract User);
    private sealed record AuthUserContract(Guid Id, string Email, IReadOnlyList<string> Roles);
}
