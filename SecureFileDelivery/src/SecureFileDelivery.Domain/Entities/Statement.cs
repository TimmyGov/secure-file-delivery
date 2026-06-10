using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Domain.Entities;

public class Statement
{
    public Guid Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    private Statement()
    {
    }

    public Statement(Guid id, CustomerId customerId, string fileName, string storagePath, long fileSizeBytes, string contentType, DateTime uploadedAt)
    {
        Id = id;
        CustomerId = customerId;
        FileName = fileName;
        StoragePath = storagePath;
        FileSizeBytes = fileSizeBytes;
        ContentType = contentType;
        UploadedAt = uploadedAt;
    }

    public void MarkAsDeleted() => IsDeleted = true;
}
