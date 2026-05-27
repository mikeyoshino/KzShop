namespace Ecommerce.Infrastructure.Storage;

public sealed class LocalStorageOptions
{
    public const string SectionName = "Storage";

    public string LocalRoot { get; init; } = "wwwroot/uploads";

    public string PublicBasePath { get; init; } = "/uploads";
}
