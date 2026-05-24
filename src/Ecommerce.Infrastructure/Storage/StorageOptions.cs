namespace Ecommerce.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Url { get; init; } = string.Empty;

    public string Key { get; init; } = string.Empty;

    public string Bucket { get; init; } = string.Empty;
}
