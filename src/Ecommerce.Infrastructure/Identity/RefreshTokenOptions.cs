namespace Ecommerce.Infrastructure.Identity;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshTokens";

    public int Days { get; init; } = 14;
}
