using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using App.Api.Options;
using Microsoft.Extensions.Options;

namespace App.Api.Services;

public class AzureObjectStorageService : IObjectStorageService
{
    private readonly AzureObjectStorageOptions _options;
    private readonly BlobServiceClient _serviceClient;
    private readonly BlobContainerClient _containerClient;
    private readonly StorageSharedKeyCredential _credential;

    public AzureObjectStorageService(IOptions<ObjectStorageOptions> objectStorageOptions)
    {
        _options = objectStorageOptions.Value.Azure;
        _credential = new StorageSharedKeyCredential(_options.AccountName, _options.AccountKey);
        _serviceClient = new BlobServiceClient(new Uri(_options.ServiceUri), _credential);
        _containerClient = _serviceClient.GetBlobContainerClient(_options.Container);
        _containerClient.CreateIfNotExists();
    }

    public async Task EnsureCorsAsync(IEnumerable<string> allowedOrigins)
    {
        var origins = allowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0)
        {
            return;
        }

        var properties = (await _serviceClient.GetPropertiesAsync()).Value;
        var expectedRule = new BlobCorsRule
        {
            AllowedOrigins = string.Join(",", origins),
            AllowedMethods = "GET,HEAD,PUT,OPTIONS",
            AllowedHeaders = "content-type,x-ms-blob-type,x-ms-blob-content-type,x-ms-version,x-ms-date,authorization",
            ExposedHeaders = "ETag,Last-Modified,x-ms-request-id,x-ms-version,x-ms-meta-*",
            MaxAgeInSeconds = 3600
        };

        if (properties.Cors.Count == 1 && CorsRuleMatches(properties.Cors[0], expectedRule))
        {
            return;
        }

        properties.Cors.Clear();
        properties.Cors.Add(expectedRule);

        await _serviceClient.SetPropertiesAsync(properties);
    }

    public Task<ObjectStorageUploadTarget> CreateUploadTarget(string storageKey, string contentType, TimeSpan lifetime)
    {
        var sasBuilder = CreateBlobSasBuilder(storageKey, lifetime);
        sasBuilder.ContentType = contentType;
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        return Task.FromResult(new ObjectStorageUploadTarget
        {
            UploadUrl = BuildBlobUrl(storageKey, sasBuilder),
            Headers = new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob"
            }
        });
    }

    public Task<string> CreateDownloadUrl(string storageKey, TimeSpan lifetime)
    {
        var sasBuilder = CreateBlobSasBuilder(storageKey, lifetime);
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(BuildBlobUrl(storageKey, sasBuilder));
    }

    public async Task Delete(string storageKey)
    {
        await _containerClient.DeleteBlobIfExistsAsync(storageKey);
    }

    private BlobSasBuilder CreateBlobSasBuilder(string storageKey, TimeSpan lifetime)
    {
        return new BlobSasBuilder
        {
            BlobContainerName = _options.Container,
            BlobName = storageKey,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.Add(lifetime)
        };
    }

    private string BuildBlobUrl(string storageKey, BlobSasBuilder sasBuilder)
    {
        var blobUri = new BlobUriBuilder(GetPublicBlobUri(storageKey))
        {
            Sas = sasBuilder.ToSasQueryParameters(_credential)
        };

        return blobUri.ToUri().ToString();
    }

    private Uri GetPublicBlobUri(string storageKey)
    {
        var baseUri = string.IsNullOrWhiteSpace(_options.PublicServiceUri)
            ? _containerClient.Uri.ToString().TrimEnd('/')
            : $"{_options.PublicServiceUri.TrimEnd('/')}/{_options.Container}";

        return new Uri($"{baseUri}/{EncodeBlobName(storageKey)}");
    }

    private static string EncodeBlobName(string blobName)
    {
        return string.Join('/', blobName.Split('/').Select(Uri.EscapeDataString));
    }

    private static bool CorsRuleMatches(BlobCorsRule actual, BlobCorsRule expected)
    {
        return string.Equals(actual.AllowedOrigins, expected.AllowedOrigins, StringComparison.OrdinalIgnoreCase)
            && string.Equals(actual.AllowedMethods, expected.AllowedMethods, StringComparison.OrdinalIgnoreCase)
            && string.Equals(actual.AllowedHeaders, expected.AllowedHeaders, StringComparison.OrdinalIgnoreCase)
            && string.Equals(actual.ExposedHeaders, expected.ExposedHeaders, StringComparison.OrdinalIgnoreCase)
            && actual.MaxAgeInSeconds == expected.MaxAgeInSeconds;
    }
}