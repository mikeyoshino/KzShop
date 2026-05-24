namespace Ecommerce.Infrastructure.Auth;

public sealed class SupabaseJwtOptions
{
    public const string SectionName = "Supabase";

    public string Url { get; init; } = string.Empty;
    public string JwtIssuer { get; init; } = string.Empty;
}
