using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using App.Api.Entities;
using App.Api.Models;
using App.Api.Options;
using App.Api.Repositories;
using Microsoft.Extensions.Options;

namespace App.Api.Services;

public class DocumentService(
    IDocumentsRepository documentsRepository,
    IAmazonS3 s3Client,
    IOptions<ObjectStorageOptions> objectStorageOptions) : IDocumentService
{
    private readonly IDocumentsRepository _documentsRepository = documentsRepository;
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly ObjectStorageOptions _objectStorageOptions = objectStorageOptions.Value;
    private static readonly TimeSpan PresignedUrlLifetime = TimeSpan.FromMinutes(15);

    // Presigned URLs must be signed against the browser-reachable endpoint, not the internal docker service URL.
    private IAmazonS3 CreatePresigningClient()
    {
        return new AmazonS3Client(
            new BasicAWSCredentials(_objectStorageOptions.AccessKey, _objectStorageOptions.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = _objectStorageOptions.PublicServiceUrl,
                AuthenticationRegion = _objectStorageOptions.Region,
                ForcePathStyle = true
            });
    }

    // The SDK always signs presigned URLs as https regardless of ServiceURL's scheme, so rewrite it back to match PublicServiceUrl.
    private string EnforcePublicUrlScheme(string presignedUrl)
    {
        var publicUri = new Uri(_objectStorageOptions.PublicServiceUrl);
        var builder = new UriBuilder(presignedUrl) { Scheme = publicUri.Scheme, Port = publicUri.Port };

        return builder.Uri.ToString();
    }

    public async Task<CreateUploadUrlResponse> CreateUploadUrl(CreateUploadUrlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("FileName is required");
        if (string.IsNullOrWhiteSpace(request.ContentType))
            throw new ArgumentException("ContentType is required");

        var storageKey = $"documents/{Guid.NewGuid()}/{request.FileName}";

        using var presigningClient = CreatePresigningClient();
        var uploadUrl = await presigningClient.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _objectStorageOptions.Bucket,
            Key = storageKey,
            Verb = HttpVerb.PUT,
            ContentType = request.ContentType,
            Expires = DateTime.UtcNow.Add(PresignedUrlLifetime)
        });

        return new CreateUploadUrlResponse
        {
            UploadUrl = EnforcePublicUrlScheme(uploadUrl),
            StorageKey = storageKey
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

        using var presigningClient = CreatePresigningClient();
        var downloadUrl = await presigningClient.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _objectStorageOptions.Bucket,
            Key = document.StorageKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(PresignedUrlLifetime)
        });

        return new DownloadUrlResponse { DownloadUrl = EnforcePublicUrlScheme(downloadUrl) };
    }

    public async Task<bool> DeleteDocument(int id)
    {
        var document = await _documentsRepository.GetById(id);
        if (document == null)
            return false;

        await _s3Client.DeleteObjectAsync(_objectStorageOptions.Bucket, document.StorageKey);
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
