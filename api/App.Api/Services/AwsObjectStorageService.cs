using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using App.Api.Options;
using Microsoft.Extensions.Options;

namespace App.Api.Services;

public class AwsObjectStorageService : IObjectStorageService
{
    private readonly AwsObjectStorageOptions _options;
    private readonly IAmazonS3 _serviceClient;

    public AwsObjectStorageService(IOptions<ObjectStorageOptions> objectStorageOptions)
    {
        _options = objectStorageOptions.Value.Aws;
        _serviceClient = CreateClient(_options.ServiceUrl);
    }

    public async Task EnsureBucketAndCorsAsync(IEnumerable<string> allowedOrigins)
    {
        if (!await AmazonS3Util.DoesS3BucketExistV2Async(_serviceClient, _options.Bucket))
        {
            await _serviceClient.PutBucketAsync(new PutBucketRequest
            {
                BucketName = _options.Bucket,
                BucketRegionName = _options.Region,
                UseClientRegion = true
            });
        }

        var origins = allowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Length == 0)
        {
            return;
        }

        var expectedConfiguration = new CORSConfiguration
        {
            Rules =
            [
                new CORSRule
                {
                    AllowedHeaders = ["*"],
                    AllowedMethods = ["GET", "HEAD", "PUT"],
                    AllowedOrigins = origins.ToList(),
                    ExposeHeaders = ["ETag"],
                    MaxAgeSeconds = 3600
                }
            ]
        };

        var currentConfiguration = await GetBucketCorsConfigurationAsync();
        if (CorsConfigurationMatches(currentConfiguration, expectedConfiguration))
        {
            return;
        }

        await _serviceClient.PutCORSConfigurationAsync(new PutCORSConfigurationRequest
        {
            BucketName = _options.Bucket,
            Configuration = expectedConfiguration
        });
    }

    public async Task<ObjectStorageUploadTarget> CreateUploadTarget(string storageKey, string contentType, TimeSpan lifetime)
    {
        using var presigningClient = CreateClient(GetPresigningServiceUrl());
        var uploadUrl = await presigningClient.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = storageKey,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(lifetime)
        });

        return new ObjectStorageUploadTarget
        {
            UploadUrl = RewriteToPublicUrl(uploadUrl),
            Headers = []
        };
    }

    public async Task<string> CreateDownloadUrl(string storageKey, TimeSpan lifetime)
    {
        using var presigningClient = CreateClient(GetPresigningServiceUrl());
        var downloadUrl = await presigningClient.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = storageKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(lifetime)
        });

        return RewriteToPublicUrl(downloadUrl);
    }

    public Task Delete(string storageKey)
    {
        return _serviceClient.DeleteObjectAsync(_options.Bucket, storageKey);
    }

    private IAmazonS3 CreateClient(string? serviceUrl)
    {
        var config = new AmazonS3Config
        {
            AuthenticationRegion = _options.Region
        };

        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            config.ServiceURL = serviceUrl;
            config.UseHttp = serviceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            config.ForcePathStyle = _options.ForcePathStyle;
        }

        return new AmazonS3Client(
            new BasicAWSCredentials(_options.AccessKey, _options.SecretKey),
            config);
    }

    private string? GetPresigningServiceUrl()
    {
        return string.IsNullOrWhiteSpace(_options.PublicServiceUrl)
            ? _options.ServiceUrl
            : _options.PublicServiceUrl;
    }

    private string RewriteToPublicUrl(string presignedUrl)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicServiceUrl))
            return presignedUrl;

        var publicUri = new Uri(_options.PublicServiceUrl);
        var builder = new UriBuilder(presignedUrl)
        {
            Scheme = publicUri.Scheme,
            Host = publicUri.Host,
            Port = publicUri.Port
        };

        return builder.Uri.ToString();
    }

    private async Task<CORSConfiguration?> GetBucketCorsConfigurationAsync()
    {
        try
        {
            var response = await _serviceClient.GetCORSConfigurationAsync(new GetCORSConfigurationRequest
            {
                BucketName = _options.Bucket
            });

            return response.Configuration;
        }
        catch (AmazonS3Exception exception) when (exception.ErrorCode is "NoSuchCORSConfiguration" or "NoSuchCORSConfigurationException")
        {
            return null;
        }
    }

    private static bool CorsConfigurationMatches(CORSConfiguration? actual, CORSConfiguration expected)
    {
        if (actual?.Rules.Count != expected.Rules.Count)
        {
            return false;
        }

        var actualRule = actual.Rules[0];
        var expectedRule = expected.Rules[0];

        return SequenceEqual(actualRule.AllowedHeaders, expectedRule.AllowedHeaders)
            && SequenceEqual(actualRule.AllowedMethods, expectedRule.AllowedMethods)
            && SequenceEqual(actualRule.AllowedOrigins, expectedRule.AllowedOrigins)
            && SequenceEqual(actualRule.ExposeHeaders, expectedRule.ExposeHeaders)
            && actualRule.MaxAgeSeconds == expectedRule.MaxAgeSeconds;
    }

    private static bool SequenceEqual(List<string>? actual, List<string>? expected)
    {
        if (actual is null || expected is null)
        {
            return actual is null && expected is null;
        }

        return actual.SequenceEqual(expected, StringComparer.OrdinalIgnoreCase);
    }
}