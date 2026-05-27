using System.Security.Claims;
using Ecommerce.Api.Auth;
using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName = "toyshops_refresh";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtOptions> jwtOptions,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _jwtOptions = jwtOptions.Value;
        _environment = environment;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var refreshToken = await _refreshTokenService.CreateAsync(user.Id, cancellationToken);
        SetRefreshCookie(refreshToken.RawToken, refreshToken.Entity.ExpiresAtUtc);

        return BuildAuthResponse(user, roles.ToArray());
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var rawRefreshToken))
        {
            return Unauthorized();
        }

        var existingRefreshToken = await _refreshTokenService.FindActiveAsync(rawRefreshToken, cancellationToken);
        if (existingRefreshToken is null)
        {
            ClearRefreshCookie();
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(existingRefreshToken.UserId.ToString());
        if (user is null)
        {
            ClearRefreshCookie();
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var replacement = await _refreshTokenService.CreateAsync(user.Id, cancellationToken);
        await _refreshTokenService.RevokeAsync(existingRefreshToken, replacement.Entity.Id, cancellationToken);
        SetRefreshCookie(replacement.RawToken, replacement.Entity.ExpiresAtUtc);

        return BuildAuthResponse(user, roles.ToArray());
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var rawRefreshToken))
        {
            var existingRefreshToken = await _refreshTokenService.FindActiveAsync(rawRefreshToken, cancellationToken);
            if (existingRefreshToken is not null)
            {
                await _refreshTokenService.RevokeAsync(existingRefreshToken, null, cancellationToken);
            }
        }

        ClearRefreshCookie();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new AuthUserResponse(user.Id, user.Email ?? string.Empty, roles.ToArray());
    }

    private AuthResponse BuildAuthResponse(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        var accessToken = _jwtTokenService.CreateAccessToken(user, roles);
        return new AuthResponse(
            accessToken,
            DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            new AuthUserResponse(user.Id, user.Email ?? string.Empty, roles.ToArray()));
    }

    private void SetRefreshCookie(string rawToken, DateTimeOffset expiresAtUtc)
    {
        Response.Cookies.Append(RefreshCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !(_environment.IsDevelopment() || _environment.IsEnvironment("Test")),
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = expiresAtUtc
        });
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            Path = "/api/auth"
        });
    }
}
