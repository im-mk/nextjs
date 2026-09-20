namespace App.Api.Options;

public class ObjectStorageOptions
{
    public string Provider { get; set; } = ObjectStorageProviders.Azure;
    public AzureObjectStorageOptions Azure { get; set; } = new();
    public AwsObjectStorageOptions Aws { get; set; } = new();
}

public static class ObjectStorageProviders
{
    public const string Azure = "Azure";
    public const string Aws = "Aws";
}

public class AzureObjectStorageOptions
{
    public string ServiceUri { get; set; } = default!;
    public string PublicServiceUri { get; set; } = default!;
    public string AccountName { get; set; } = default!;
    public string AccountKey { get; set; } = default!;
    public string Container { get; set; } = default!;
}

public class AwsObjectStorageOptions
{
    public string ServiceUrl { get; set; } = default!;
    public string PublicServiceUrl { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Bucket { get; set; } = default!;
    public string Region { get; set; } = "eu-west-2";
    public bool ForcePathStyle { get; set; }
}
