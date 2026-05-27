namespace Ecommerce.Infrastructure.Identity;

public interface IRefreshTokenService
{
    Task<(string RawToken, RefreshToken Entity)> CreateAsync(Guid userId, CancellationToken cancellationToken);

    Task<RefreshToken?> FindActiveAsync(string rawToken, CancellationToken cancellationToken);

    Task RevokeAsync(RefreshToken token, Guid? replacedByTokenId, CancellationToken cancellationToken);
}
