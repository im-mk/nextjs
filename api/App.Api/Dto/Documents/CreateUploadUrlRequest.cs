namespace App.Api.Dto.Documents;

public class CreateUploadUrlRequest
{
    public int? ContactId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
}