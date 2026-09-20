using App.Api.Options;
using App.Api.Repositories;
using App.Api.Services;
using Npgsql;
using Serilog;
using System.Data;
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
var allowedOrigins = GetAllowedOrigins(builder.Configuration);
AddCors(builder, allowedOrigins);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

await ConfigureObjectStorageAsync(app, allowedOrigins);

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

static string[] GetAllowedOrigins(ConfigurationManager configuration)
{
    return new[]
    {
        "http://localhost:3000",
        "http://localhost:8090",
        configuration["Cors:WebOrigin"] ?? string.Empty
    }.Where(origin => !string.IsNullOrWhiteSpace(origin)).ToArray();
}

static void AddCors(WebApplicationBuilder builder, string[] allowedOrigins)
{
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

static async Task ConfigureObjectStorageAsync(WebApplication app, string[] allowedOrigins)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    var storageService = app.Services.GetRequiredService<IObjectStorageService>();

    if (storageService is not AzureObjectStorageService azureStorageService)
    {
        if (storageService is AwsObjectStorageService awsObjectStorageService)
        {
            await awsObjectStorageService.EnsureBucketAndCorsAsync(allowedOrigins);
        }

        return;
    }

    await azureStorageService.EnsureCorsAsync(allowedOrigins);
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

    builder.Services.AddSingleton<IObjectStorageService>(sp =>
    {
        var options = builder.Configuration.GetSection("ObjectStorage").Get<ObjectStorageOptions>()
            ?? throw new InvalidOperationException("ObjectStorage configuration is missing");

        if (string.Equals(options.Provider, ObjectStorageProviders.Aws, StringComparison.OrdinalIgnoreCase))
            return new AwsObjectStorageService(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ObjectStorageOptions>>());

        return new AzureObjectStorageService(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ObjectStorageOptions>>());
    });
}
