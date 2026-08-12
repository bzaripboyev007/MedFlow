namespace MedFlow.Application.Common.Interfaces;

/// <summary>
/// Retsept fayllarini saqlash abstraksiyasi. Hozircha mahalliy disk implementatsiyasi ishlatiladi (dev);
/// production'da shu interfeys orqasida MinIO + presigned URL implementatsiyasi almashtiriladi (spec 5a/7),
/// bu bosqichda MinIO muhitda mavjud emas.
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default);
    string GetUrl(string storageKey);
}
