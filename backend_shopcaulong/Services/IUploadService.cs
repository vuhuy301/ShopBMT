// Services/IUploadService.cs
namespace backend_shopcaulong.Services
{
    public interface IUploadService
    {
        Task<List<string>> UploadProductImagesAsync(IFormFileCollection files);
        Task<string?> UploadDetailImageAsync(IFormFile? file);
        Task<string?> UploadVariantImageAsync(IFormFile? file);
    }
}