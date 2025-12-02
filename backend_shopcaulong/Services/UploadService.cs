// Services/UploadService.cs
using Microsoft.AspNetCore.Http;
using System.IO;

namespace backend_shopcaulong.Services
{
    public class UploadService : IUploadService
    {
        private readonly IWebHostEnvironment _env;

        public UploadService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // 1. Ảnh chính sản phẩm → lưu vào wwwroot/images/products/
        public async Task<List<string>> UploadProductImagesAsync(IEnumerable<IFormFile> files)
        {
            return await UploadFilesAsync(files, "images/products");
        }

        // 2. Ảnh biến thể màu → lưu vào wwwroot/images/products/variants/
        public async Task<List<string>> UploadVariantImagesAsync(IEnumerable<IFormFile> files)
        {
            return await UploadFilesAsync(files, "images/products/variants");
        }

        // 3. Ảnh chi tiết mô tả → lưu vào wwwroot/images/products/details/
        public async Task<string?> UploadDetailImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var folder = Path.Combine(_env.WebRootPath, "images", "products", "details");
            Directory.CreateDirectory(folder); // Đảm bảo tạo thư mục

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                throw new InvalidOperationException("Định dạng file không được phép");

            var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(file.FileName) + ext);
            var finalName = GetUniqueFileName(folder, safeName);
            var filePath = Path.Combine(folder, finalName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/details/{finalName}";
        }

        // HÀM CHUNG – ĐÃ SỬA ĐÚNG 100% ĐỂ LUÔN TẠO ĐƯỢC THƯ MỤC VÀ LƯU ĐÚNG ĐƯỜNG DẪN
        private async Task<List<string>> UploadFilesAsync(IEnumerable<IFormFile> files, string relativePath)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("Không có file nào được gửi lên");

            // Chuẩn hóa đường dẫn: "images/products/variants" → wwwroot/images/products/variants
            var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
            Directory.CreateDirectory(fullPath); // Tạo thư mục nếu chưa có (tạo từng cấp)

            var urls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                    throw new InvalidOperationException($"Định dạng file không được phép: {file.FileName}");

                var originalName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("&", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("#", "");

                var safeFileName = SanitizeFileName(originalName + ext);
                var finalFileName = GetUniqueFileName(fullPath, safeFileName);
                var filePath = Path.Combine(fullPath, finalFileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Trả về URL chuẩn (dấu / thay vì \)
                var url = $"/{relativePath.TrimStart('/').Replace("\\", "/")}/{finalFileName}";
                urls.Add(url);
            }

            return urls;
        }

        // Xóa file
        public void DeleteFile(string? fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            try
            {
                var filePath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/'));
                filePath = filePath.Replace("/", Path.DirectorySeparatorChar.ToString());
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { /* ignore */ }
        }

        // Làm sạch tên file
        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        // Tránh trùng tên
        private static string GetUniqueFileName(string folder, string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var result = fileName;
            int count = 1;

            while (File.Exists(Path.Combine(folder, result)))
            {
                result = $"{name}({count++}){ext}";
            }

            return result;
        }
    }
}