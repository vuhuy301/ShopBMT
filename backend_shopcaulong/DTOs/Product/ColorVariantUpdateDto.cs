namespace backend_shopcaulong.DTOs.Product
{
    public class ColorVariantUpdateDto
    {
        public int Id { get; set; } // 0 nếu thêm mới
        public string Color { get; set; }

        public List<string>? ImageUrls { get; set; } = new();  // Ảnh cũ
        public IFormFileCollection? ImageFiles { get; set; }   // Ảnh mới

        public List<SizeVariantUpdateDto> Sizes { get; set; } = new();
    }

}
