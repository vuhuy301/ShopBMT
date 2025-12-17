// Services/IUploadService.cs
namespace backend_shopcaulong.Services
{
public interface IUploadService
{
    Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files);
    Task<string?> UploadDetailImageAsync(IFormFile? file);
    Task<List<string>> UploadVariantImagesAsync(IEnumerable<IFormFile> files);
    Task<string?> UploadBannerImageAsync(IFormFile? file);
    void DeleteFile(string? fileUrl);
}
}