using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Infrastructure.Storage;

public sealed class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly SemaphoreSlim _bucketSemaphore = new(1, 1);
    private bool _bucketEnsured;

    public MinioFileStorage(IMinioClient minioClient, IOptions<MinioSettings> options)
    {
        _minioClient = minioClient;
        _settings = options.Value;
    }

    public async Task<string> UploadAsync(Stream stream, string filename, string contentType)
    {
        await EnsureBucketExistsAsync();
        var objectName = $"{Guid.NewGuid():N}{Path.GetExtension(filename)}";

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var objectSize = stream.CanSeek ? stream.Length - stream.Position : -1;

        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(objectSize)
            .WithContentType(contentType));

        return objectName;
    }

    public async Task<Stream> DownloadAsync(string storagePath)
    {
        await EnsureBucketExistsAsync();
        var memoryStream = new MemoryStream();
        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(storagePath)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream)));
        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteAsync(string storagePath)
    {
        await EnsureBucketExistsAsync();
        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(storagePath));
    }

    private async Task EnsureBucketExistsAsync()
    {
        if (_bucketEnsured)
        {
            return;
        }

        await _bucketSemaphore.WaitAsync();
        try
        {
            if (_bucketEnsured)
            {
                return;
            }

            var exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName));
            if (!exists)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName));
            }

            _bucketEnsured = true;
        }
        finally
        {
            _bucketSemaphore.Release();
        }
    }
}
