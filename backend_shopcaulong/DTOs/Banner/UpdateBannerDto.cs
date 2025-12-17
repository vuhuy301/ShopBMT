using Microsoft.AspNetCore.Http;

namespace backend_shopcaulong.DTOs.Banner
{
    public class UpdateBannerDto
    {
        public IFormFile? Image { get; set; } // có thể không đổi ảnh
        public string? Link { get; set; }
        public bool IsActive { get; set; }
    }
}
