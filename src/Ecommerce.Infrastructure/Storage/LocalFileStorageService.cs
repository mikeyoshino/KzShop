using Ecommerce.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Storage;

public sealed class LocalFileStorageService : IStorageService
{
    private readonly IHostEnvironment _environment;
    private readonly LocalStorageOptions _options;

    public LocalFileStorageService(IHostEnvironment environment, IOptions<LocalStorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            throw new ArgumentException("Object path is required.", nameof(objectPath));
        }

        var relativePath = NormalizeObjectPath(objectPath);
        var absoluteRoot = GetAbsoluteRoot();
        var absolutePath = Path.GetFullPath(Path.Combine(absoluteRoot, relativePath));
        EnsurePathIsWithinRoot(absoluteRoot, absolutePath);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = File.Create(absolutePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"{_options.PublicBasePath.TrimEnd('/')}/{relativePath.Replace('\\', '/')}";
    }

    public Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            return Task.CompletedTask;
        }

        var relativePath = objectPath;
        var publicBasePath = _options.PublicBasePath.TrimEnd('/');
        if (relativePath.StartsWith(publicBasePath, StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath[publicBasePath.Length..];
        }

        relativePath = NormalizeObjectPath(relativePath);
        var absoluteRoot = GetAbsoluteRoot();
        var absolutePath = Path.GetFullPath(Path.Combine(absoluteRoot, relativePath));
        EnsurePathIsWithinRoot(absoluteRoot, absolutePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetAbsoluteRoot()
    {
        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.LocalRoot));
    }

    private static string NormalizeObjectPath(string objectPath)
    {
        var normalized = objectPath
            .Replace('\\', '/')
            .Trim()
            .TrimStart('/');

        if (normalized.Length == 0 ||
            normalized.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new ArgumentException("Object path is invalid.", nameof(objectPath));
        }

        return normalized;
    }

    private static void EnsurePathIsWithinRoot(string absoluteRoot, string absolutePath)
    {
        var root = absoluteRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!absolutePath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Object path escapes the storage root.", nameof(absolutePath));
        }
    }
}
