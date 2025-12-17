using Microsoft.AspNetCore.Http;

namespace backend_shopcaulong.DTOs.Banner
{
    public class CreateBannerDto
    {
        public IFormFile Image { get; set; } = null!;
        public string? Link { get; set; }
        public bool IsActive { get; set; }
    }
}
