namespace backend_shopcaulong.DTOs.Banner
{
    public class BannerDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? Link { get; set; }
        public bool IsActive { get; set; }
    }
}