using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        }

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services
            .AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName))
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Url) &&
                !string.IsNullOrWhiteSpace(options.Key) &&
                !string.IsNullOrWhiteSpace(options.Bucket),
                "Storage options Url, Key, and Bucket are required.")
            .Validate(options =>
                Uri.TryCreate(options.Url, UriKind.Absolute, out var storageUri) &&
                (storageUri.Scheme == Uri.UriSchemeHttp || storageUri.Scheme == Uri.UriSchemeHttps),
                "Storage Url must be a valid absolute HTTP or HTTPS URI.");

        services.AddHttpClient<IStorageService, SupabaseStorageService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;
            client.BaseAddress = new Uri(options.Url.TrimEnd('/'));
        });

        return services;
    }
}
