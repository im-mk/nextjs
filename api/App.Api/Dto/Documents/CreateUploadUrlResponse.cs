namespace App.Api.Dto.Documents;

public class CreateUploadUrlResponse
{
    public string UploadUrl { get; set; } = default!;
    public string StorageKey { get; set; } = default!;
    public Dictionary<string, string> UploadHeaders { get; set; } = [];
}