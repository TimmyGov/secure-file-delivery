namespace SecureFileDelivery.Domain.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(Stream stream, string filename, string contentType);
    Task<Stream> DownloadAsync(string storagePath);
    Task DeleteAsync(string storagePath);
}
