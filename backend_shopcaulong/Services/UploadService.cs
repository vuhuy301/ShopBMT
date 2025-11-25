// Services/UploadService.cs
using Microsoft.AspNetCore.Http;

namespace backend_shopcaulong.Services
{
    public class UploadService : IUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UploadService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<string>> UploadProductImagesAsync(IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                throw new ArgumentException("Không có file nào được gửi lên");

            var uploadFolder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadFolder);

            var urls = new List<string>();
            var request = _httpContextAccessor.HttpContext!.Request;

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                    throw new InvalidOperationException($"File {file.FileName} không được phép");

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Tạo URL đầy đủ – cực kỳ quan trọng cho .NET 6
                var url = $"{request.Scheme}://{request.Host}/images/products/{fileName}";
                urls.Add(url);
            }

            return urls;
        }

        public async Task<string?> UploadDetailImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                throw new InvalidOperationException($"File {file.FileName} không được phép");

            var detailFolder = Path.Combine(_env.WebRootPath, "images", "products", "details");
            Directory.CreateDirectory(detailFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(detailFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var request = _httpContextAccessor.HttpContext!.Request;
            return $"{request.Scheme}://{request.Host}/images/products/details/{fileName}";
        }
        public async Task<string?> UploadVariantImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var folder = Path.Combine(_env.WebRootPath, "images", "products", "variants");
            Directory.CreateDirectory(folder);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var request = _httpContextAccessor.HttpContext!.Request;
            return $"{request.Scheme}://{request.Host}/images/products/variants/{fileName}";
        }
    }
}