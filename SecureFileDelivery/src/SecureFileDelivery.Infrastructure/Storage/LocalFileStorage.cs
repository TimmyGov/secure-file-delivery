using Microsoft.Extensions.Options;
using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IOptions<LocalStorageSettings> options)
    {
        _basePath = options.Value.BasePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream stream, string filename, string contentType)
    {
        var extension = Path.GetExtension(filename);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_basePath, storedFileName);

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await using var fileStream = File.Create(fullPath);
        await stream.CopyToAsync(fileStream);
        return storedFileName;
    }

    public async Task<Stream> DownloadAsync(string storagePath)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        var memoryStream = new MemoryStream(await File.ReadAllBytesAsync(fullPath));
        memoryStream.Position = 0;
        return memoryStream;
    }

    public Task DeleteAsync(string storagePath)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
