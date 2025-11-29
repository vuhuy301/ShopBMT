// // Services/UploadService.cs
// using Microsoft.AspNetCore.Http;

// namespace backend_shopcaulong.Services
// {
//     public class UploadService : IUploadService
//     {
//         private readonly IWebHostEnvironment _env;

//         public UploadService(IWebHostEnvironment env)
//         {
//             _env = env;
//         }

//         // ĐÃ SỬA: nhận IEnumerable<IFormFile> → chấp nhận cả List<IFormFile> và IFormFileCollection
//         public async Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files)
//         {
//             if (files == null || !files.Any())
//                 throw new ArgumentException("Không có file nào được gửi lên");

//             var folder = Path.Combine(_env.WebRootPath, "images", "products");
//             Directory.CreateDirectory(folder);

//             var urls = new List<string>();

//             foreach (var file in files)
//             {
//                 if (file.Length == 0) continue;

//                 var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//                 if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
//                     throw new InvalidOperationException($"File không được phép: {file.FileName}");

//                 var fileName = $"{Guid.NewGuid()}{ext}";
//                 var filePath = Path.Combine(folder, fileName);

//                 await using var stream = new FileStream(filePath, FileMode.Create);
//                 await file.CopyToAsync(stream);

//                 urls.Add($"/images/products/{fileName}");
//             }

//             return urls;
//         }

//         public async Task<string?> UploadDetailImageAsync(IFormFile? file)
//         {
//             if (file == null || file.Length == 0) return null;

//             var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//             if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
//                 throw new InvalidOperationException("File không được phép");

//             var folder = Path.Combine(_env.WebRootPath, "images", "products", "details");
//             Directory.CreateDirectory(folder);

//             var fileName = $"{Guid.NewGuid()}{ext}";
//             var filePath = Path.Combine(folder, fileName);

//             await using var stream = new FileStream(filePath, FileMode.Create);
//             await file.CopyToAsync(stream);

//             return $"/images/products/details/{fileName}";
//         }

//         public void DeleteFile(string? fileUrl)
//         {
//             if (string.IsNullOrWhiteSpace(fileUrl)) return;
//             try
//             {
//                 var filePath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/'));
//                 if (File.Exists(filePath)) File.Delete(filePath);
//             }
//             catch { /* ignore */ }
//         }
//     }
// }

// Services/UploadService.cs
using Microsoft.AspNetCore.Http;

namespace backend_shopcaulong.Services
{
    public class UploadService : IUploadService
    {
        private readonly IWebHostEnvironment _env;

        public UploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("Không có file nào được gửi lên");

            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            var urls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                    throw new InvalidOperationException($"Định dạng file không được phép: {file.FileName}");

                // LẤY TÊN FILE GỐC (không có ký tự lạ)
                var originalName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("&", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("#", "");

                var safeFileName = SanitizeFileName(originalName + ext);
                var finalFileName = GetUniqueFileName(folder, safeFileName);
                var filePath = Path.Combine(folder, finalFileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                urls.Add($"/images/products/{finalFileName}");
            }

            return urls;
        }

        public async Task<string?> UploadDetailImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                throw new InvalidOperationException("Định dạng file không được phép");

            var folder = Path.Combine(_env.WebRootPath, "images", "products", "details");
            Directory.CreateDirectory(folder);

            var originalName = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace(" ", "_");

            var safeFileName = SanitizeFileName(originalName + ext);
            var finalFileName = GetUniqueFileName(folder, safeFileName);
            var filePath = Path.Combine(folder, finalFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/details/{finalFileName}";
        }

        public void DeleteFile(string? fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/').Replace("/", "\\"));
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { /* ignore */ }
        }

        // === HÀM HỖ TRỢ ===
        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        private static string GetUniqueFileName(string folder, string fileName)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var finalName = fileName;
            var counter = 1;

            while (File.Exists(Path.Combine(folder, finalName)))
            {
                finalName = $"{nameWithoutExt}({counter++}){ext}";
            }

            return finalName;
        }
    }
}