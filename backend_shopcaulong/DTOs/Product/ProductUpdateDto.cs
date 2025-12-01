using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.DTOs.Product
{
    public class ProductUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsFeatured { get; set; }

        // Ảnh cũ giữ lại
        public List<string>? ImageUrls { get; set; } = new();

        // Ảnh mới upload thêm
        public IFormFileCollection? ImageFiles { get; set; }

        // Chi tiết mô tả
        public List<ProductDetailUpdateDto>? Details { get; set; }

        // Biến thể
        public List<ColorVariantUpdateDto> ColorVariants { get; set; } = new();
    }

}
