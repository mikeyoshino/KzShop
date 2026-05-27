namespace Ecommerce.Infrastructure.Identity;

public interface IJwtTokenService
{
    string CreateAccessToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
