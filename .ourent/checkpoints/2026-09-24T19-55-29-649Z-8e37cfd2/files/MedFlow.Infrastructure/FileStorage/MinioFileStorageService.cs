using MedFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace MedFlow.Infrastructure.FileStorage;

/// <summary>
/// Retsept fayllarini MinIO (S3-mos) obyekt xotirasida saqlaydi. Bucket ochiq (public-read)
/// qilib qo'yilgan, shuning uchun GetUrl to'g'ridan-to'g'ri doimiy havola qaytaradi.
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    public MinioFileStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["Minio:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["Minio:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["Minio:SecretKey"] ?? "minioadmin";
        var useSsl = bool.Parse(configuration["Minio:UseSsl"] ?? "false");
        _bucketName = configuration["Minio:BucketName"] ?? "medflow-prescriptions";

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build();

        _publicBaseUrl = $"{(useSsl ? "https" : "http")}://{endpoint}/{_bucketName}";
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var storageKey = $"{Guid.NewGuid():N}{extension}";

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(storageKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType), ct);

        return storageKey;
    }

    public string GetUrl(string storageKey) => $"{_publicBaseUrl}/{storageKey}";
}
