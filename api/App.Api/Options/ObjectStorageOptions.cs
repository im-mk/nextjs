namespace App.Api.Options;

public class ObjectStorageOptions
{
    public string ServiceUrl { get; set; } = default!;
    // Browser-reachable endpoint used only for signing presigned URLs; the internal ServiceUrl (e.g. docker service name) isn't resolvable from the browser.
    public string PublicServiceUrl { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Bucket { get; set; } = default!;
    // Must match the s3_region configured in garage.toml.
    public string Region { get; set; } = "garage";
}
