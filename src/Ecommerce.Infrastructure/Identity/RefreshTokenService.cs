using System.Security.Cryptography;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Identity;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenService(ApplicationDbContext context, IOptions<RefreshTokenOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<(string RawToken, RefreshToken Entity)> CreateAsync(Guid userId, CancellationToken cancellationToken)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var token = RefreshToken.Create(
            userId,
            Hash(rawToken),
            DateTimeOffset.UtcNow.AddDays(_options.Days));

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);

        return (rawToken, token);
    }

    public async Task<RefreshToken?> FindActiveAsync(string rawToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return null;
        }

        var tokenHash = Hash(rawToken);
        var nowUtc = DateTimeOffset.UtcNow;
        var token = await _context.RefreshTokens
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        return token is not null && token.IsActive(nowUtc) ? token : null;
    }

    public async Task RevokeAsync(RefreshToken token, Guid? replacedByTokenId, CancellationToken cancellationToken)
    {
        token.Revoke(replacedByTokenId);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
