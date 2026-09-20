namespace App.Api.Services;

public interface IObjectStorageService
{
    Task<ObjectStorageUploadTarget> CreateUploadTarget(string storageKey, string contentType, TimeSpan lifetime);
    Task<string> CreateDownloadUrl(string storageKey, TimeSpan lifetime);
    Task Delete(string storageKey);
}

public class ObjectStorageUploadTarget
{
    public string UploadUrl { get; init; } = default!;
    public Dictionary<string, string> Headers { get; init; } = [];
}