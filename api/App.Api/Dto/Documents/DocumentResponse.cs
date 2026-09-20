namespace App.Api.Dto.Documents;

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