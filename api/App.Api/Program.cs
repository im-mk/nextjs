using System.Data;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Npgsql;
using App.Api.Options;
using App.Api.Repositories;
using App.Api.Services;
using Serilog;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "App API",
        Version = "v1"
    });
});

SetupDb(builder);
AddDomainDependencies(builder);
AddFusionCache(builder);
AddObjectStorage(builder);

builder.Services.AddHealthChecks();
AddCors(builder);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "App API v1"
        );

        options.RoutePrefix = "swagger";
    });
}

app.MapHealthChecks("/health");

// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

await app.StartAsync();
await EnsureBucketCorsConfigured(app);
await app.WaitForShutdownAsync();


static void SetupDb(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IDbConnection>(sp =>
    {
        var conn = new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
        conn.Open();
        return conn;
    });
}

static void AddFusionCache(WebApplicationBuilder builder)
{
    builder.Services.AddFusionCache()
        .WithDefaultEntryOptions(new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromDays(1)
        });
}

static void AddCors(WebApplicationBuilder builder)
{
    var allowedOrigins = new[]
    {
        "http://localhost:3000",
        "http://localhost:8090",
        builder.Configuration["Cors:WebOrigin"] ?? string.Empty
    }.Where(origin => !string.IsNullOrWhiteSpace(origin)).ToArray();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .WithExposedHeaders("X-Request-Id", "X-Total-Count", "X-Page", "X-Page-Size");
        });
    });
}

static void AddDomainDependencies(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
    builder.Services.AddScoped<IAddressesRepository, AddressesRepository>();
    builder.Services.AddScoped<IContactsRepository, ContactsRepository>();
    builder.Services.AddScoped<ICountriesService, CountriesService>();
    builder.Services.AddScoped<IContactService, ContactService>();
    builder.Services.AddScoped<IDocumentsRepository, DocumentsRepository>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
}

static void AddObjectStorage(WebApplicationBuilder builder)
{
    builder.Services.Configure<ObjectStorageOptions>(
        builder.Configuration.GetSection("ObjectStorage"));

    builder.Services.AddSingleton<IAmazonS3>(sp =>
    {
        var options = builder.Configuration.GetSection("ObjectStorage").Get<ObjectStorageOptions>()
            ?? throw new InvalidOperationException("ObjectStorage configuration is missing");

        return new AmazonS3Client(
            new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region,
                UseHttp = options.ServiceUrl.StartsWith("http://"),
                ForcePathStyle = true
            });
    });
}

// Presigned PUT/GET requests from the browser trigger a CORS preflight, which Garage rejects unless the bucket has a CORS policy.
static async Task EnsureBucketCorsConfigured(WebApplication app)
{
    var options = app.Configuration.GetSection("ObjectStorage").Get<ObjectStorageOptions>();
    if (options == null || string.IsNullOrWhiteSpace(options.Bucket))
        return;

    using var scope = app.Services.CreateScope();
    var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();

    try
    {
        await s3Client.PutCORSConfigurationAsync(new PutCORSConfigurationRequest
        {
            BucketName = options.Bucket,
            Configuration = new CORSConfiguration
            {
                // Separate rules per origin: Garage echoes back the full AllowedOrigins list rather than matching
                // a single origin when multiple origins share one rule, which browsers reject.
                Rules =
                [
                    new CORSRule
                    {
                        AllowedOrigins = ["http://localhost:3000"],
                        AllowedMethods = ["GET", "PUT", "POST", "DELETE", "HEAD"],
                        AllowedHeaders = ["*"],
                        MaxAgeSeconds = 3600
                    },
                    new CORSRule
                    {
                        AllowedOrigins = ["http://localhost:8090"],
                        AllowedMethods = ["GET", "PUT", "POST", "DELETE", "HEAD"],
                        AllowedHeaders = ["*"],
                        MaxAgeSeconds = 3600
                    }
                ]
            }
        });
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to configure CORS on the {Bucket} bucket", options.Bucket);
    }
}