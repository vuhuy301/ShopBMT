namespace backend_shopcaulong.DTOs.Product
{
    public class ColorVariantDto
    {
        public int Id { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();

        // File ảnh mới (nếu người dùng upload thêm/thay ảnh)
        public IFormFileCollection? ImageFiles { get; set; }
        public List<SizeVariantDto> Sizes { get; set; } = new();
    }
}