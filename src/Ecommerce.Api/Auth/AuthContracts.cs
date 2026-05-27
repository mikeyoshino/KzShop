namespace Ecommerce.Api.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthUserResponse(Guid Id, string Email, IReadOnlyList<string> Roles);

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, AuthUserResponse User);
