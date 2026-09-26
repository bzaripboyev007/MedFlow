using MedFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MedFlow.Infrastructure.FileStorage;

/// <summary>
/// Dev-uchun mahalliy diskka yozadigan implementatsiya. Production'da shu interfeys
/// orqasida MinIO + presigned URL bilan almashtiriladi (spec bo'lim 5a/7).
/// </summary>
public class LocalDiskFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _publicBasePath;

    public LocalDiskFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "uploads");
        _publicBasePath = configuration["FileStorage:PublicBasePath"] ?? "/uploads";
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var storageKey = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_rootPath, storageKey);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        return storageKey;
    }

    public string GetUrl(string storageKey) => $"{_publicBasePath.TrimEnd('/')}/{storageKey}";
}
