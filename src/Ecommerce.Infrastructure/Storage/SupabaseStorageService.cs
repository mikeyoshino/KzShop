using System.Net.Http.Headers;
using Ecommerce.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Storage;

public sealed class SupabaseStorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly StorageOptions _options;

    public SupabaseStorageService(HttpClient httpClient, IOptions<StorageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        EnsureStorageOptionsAreValid(_options);
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

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }

        var request = new HttpRequestMessage(HttpMethod.Post, BuildObjectUri(objectPath))
        {
            Content = new StreamContent(content)
        };

        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Headers.Add("x-upsert", "true");
        AddAuthHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return objectPath;
    }

    public async Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            throw new ArgumentException("Object path is required.", nameof(objectPath));
        }

        var request = new HttpRequestMessage(HttpMethod.Delete, BuildObjectUri(objectPath));
        AddAuthHeaders(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private Uri BuildObjectUri(string objectPath)
    {
        var baseUrl = _options.Url.TrimEnd('/');
        var escapedBucket = Uri.EscapeDataString(_options.Bucket);
        var escapedPath = string.Join(
            '/',
            objectPath.Split('/').Select(Uri.EscapeDataString));

        return new Uri($"{baseUrl}/storage/v1/object/{escapedBucket}/{escapedPath}");
    }

    private void AddAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("apikey", _options.Key);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Key);
    }

    private static void EnsureStorageOptionsAreValid(StorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new InvalidOperationException("Storage configuration error: Storage:Url is required.");
        }

        if (!Uri.TryCreate(options.Url, UriKind.Absolute, out var storageUri) ||
            (storageUri.Scheme != Uri.UriSchemeHttp && storageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "Storage configuration error: Storage:Url must be a valid absolute HTTP or HTTPS URI.");
        }

        if (string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("Storage configuration error: Storage:Key is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Bucket))
        {
            throw new InvalidOperationException("Storage configuration error: Storage:Bucket is required.");
        }
    }
}
