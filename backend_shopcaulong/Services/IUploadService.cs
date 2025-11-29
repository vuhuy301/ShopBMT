// Services/IUploadService.cs
namespace backend_shopcaulong.Services
{
public interface IUploadService
{
    Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files);
    Task<string?> UploadDetailImageAsync(IFormFile? file);
    void DeleteFile(string? fileUrl);
}
}