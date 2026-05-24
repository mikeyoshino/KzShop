namespace Ecommerce.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default);
}
