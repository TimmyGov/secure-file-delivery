using System.Collections.Concurrent;
using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Integration.Tests;

internal sealed class InMemoryFileStorage : IFileStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _files = new();

    public async Task<string> UploadAsync(Stream stream, string filename, string contentType)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var key = Guid.NewGuid().ToString("N");
        _files[key] = memoryStream.ToArray();
        return key;
    }

    public Task<Stream> DownloadAsync(string storagePath)
    {
        var bytes = _files[storagePath];
        Stream stream = new MemoryStream(bytes);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath)
    {
        _files.TryRemove(storagePath, out _);
        return Task.CompletedTask;
    }
}
