using App.Api.Entities;
using App.Api.Dto.Documents;
using App.Api.Repositories;

namespace App.Api.Services;

public class DocumentService(
    IDocumentsRepository documentsRepository,
    IObjectStorageService objectStorageService) : IDocumentService
{
    private readonly IDocumentsRepository _documentsRepository = documentsRepository;
    private readonly IObjectStorageService _objectStorageService = objectStorageService;
    private static readonly TimeSpan PresignedUrlLifetime = TimeSpan.FromMinutes(15);

    public async Task<CreateUploadUrlResponse> CreateUploadUrl(CreateUploadUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("FileName is required");
        if (string.IsNullOrWhiteSpace(request.ContentType))
            throw new ArgumentException("ContentType is required");

        var storageKey = $"documents/{Guid.NewGuid()}/{request.FileName}";
        var uploadTarget = await _objectStorageService.CreateUploadTarget(storageKey, request.ContentType, PresignedUrlLifetime);

        return new CreateUploadUrlResponse
        {
            UploadUrl = uploadTarget.UploadUrl,
            StorageKey = storageKey,
            UploadHeaders = uploadTarget.Headers
        };
    }

    public async Task<DocumentResponse> CompleteUpload(CompleteUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("FileName is required");
        if (string.IsNullOrWhiteSpace(request.ContentType))
            throw new ArgumentException("ContentType is required");
        if (string.IsNullOrWhiteSpace(request.StorageKey))
            throw new ArgumentException("StorageKey is required");

        var document = new Document
        {
            ContactId = request.ContactId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            StorageKey = request.StorageKey
        };

        var id = await _documentsRepository.Create(document);

        return MapToResponse(document, id);
    }

    public async Task<DocumentResponse?> GetDocument(int id)
    {
        var document = await _documentsRepository.GetById(id);
        return document == null ? null : MapToResponse(document, document.Id);
    }

    public async Task<IEnumerable<DocumentResponse>> GetAllDocuments(int? contactId = null)
    {
        var documents = await _documentsRepository.GetAll(contactId);
        return documents.Select(d => MapToResponse(d, d.Id));
    }

    public async Task<DownloadUrlResponse?> GetDownloadUrl(int id)
    {
        var document = await _documentsRepository.GetById(id);
        if (document == null)
            return null;

        var downloadUrl = await _objectStorageService.CreateDownloadUrl(document.StorageKey, PresignedUrlLifetime);

        return new DownloadUrlResponse { DownloadUrl = downloadUrl };
    }

    public async Task<bool> DeleteDocument(int id)
    {
        var document = await _documentsRepository.GetById(id);
        if (document == null)
            return false;

        await _objectStorageService.Delete(document.StorageKey);
        return await _documentsRepository.Delete(id);
    }

    private static DocumentResponse MapToResponse(Document document, int id)
    {
        return new DocumentResponse
        {
            Id = id,
            ContactId = document.ContactId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            SizeBytes = document.SizeBytes,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
