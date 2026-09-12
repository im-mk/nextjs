namespace App.Api.Models;

public class DocumentResponse
{
    public int Id { get; set; }
    public int? ContactId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUploadUrlRequest
{
    public int? ContactId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
}

public class CreateUploadUrlResponse
{
    public string UploadUrl { get; set; } = default!;
    public string StorageKey { get; set; } = default!;
}

public class CompleteUploadRequest
{
    public int? ContactId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string StorageKey { get; set; } = default!;
}

public class DownloadUrlResponse
{
    public string DownloadUrl { get; set; } = default!;
}
