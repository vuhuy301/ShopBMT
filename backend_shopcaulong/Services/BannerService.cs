
using backend_shopcaulong.DTOs.Banner;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services.Banners
{
    public class BannerService : IBannerService
    {
        private readonly ShopDbContext _context;
        private readonly IUploadService _uploadService;

        public BannerService(ShopDbContext context, IUploadService uploadService)
        {
            _context = context;
            _uploadService = uploadService;
        }

        public async Task<List<BannerDto>> GetAllAsync()
        {
            return await _context.Banners
                .Select(b => new BannerDto
                {
                    Id = b.Id,
                    ImageUrl = b.ImageUrl,
                    Link = b.Link,
                    IsActive = b.IsActive
                })
                .ToListAsync();
        }

        public async Task<BannerDto?> GetByIdAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return null;

            return new BannerDto
            {
                Id = banner.Id,
                ImageUrl = banner.ImageUrl,
                Link = banner.Link,
                IsActive = banner.IsActive
            };
        }

        public async Task<BannerDto> CreateAsync(CreateBannerDto dto)
        {
            var imageUrl = await _uploadService.UploadBannerImageAsync(dto.Image)
                           ?? throw new Exception("Upload banner image failed");

            var banner = new Banner
            {
                ImageUrl = imageUrl,
                Link = dto.Link,
                IsActive = dto.IsActive
            };

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return new BannerDto
            {
                Id = banner.Id,
                ImageUrl = banner.ImageUrl,
                Link = banner.Link,
                IsActive = banner.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateBannerDto dto)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return false;

            // Nếu có upload ảnh mới → xóa ảnh cũ
            if (dto.Image != null)
            {
                _uploadService.DeleteFile(banner.ImageUrl);
                banner.ImageUrl = await _uploadService.UploadBannerImageAsync(dto.Image)
                                  ?? banner.ImageUrl;
            }

            banner.Link = dto.Link;
            banner.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return false;

            _uploadService.DeleteFile(banner.ImageUrl);
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
