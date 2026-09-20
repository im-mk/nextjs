using App.Api.Dto.Documents;

namespace App.Api.Services;

public interface IDocumentService
{
    Task<CreateUploadUrlResponse> CreateUploadUrl(CreateUploadUrlRequest request);
    Task<DocumentResponse> CompleteUpload(CompleteUploadRequest request);
    Task<DocumentResponse?> GetDocument(int id);
    Task<IEnumerable<DocumentResponse>> GetAllDocuments(int? contactId = null);
    Task<DownloadUrlResponse?> GetDownloadUrl(int id);
    Task<bool> DeleteDocument(int id);
}
