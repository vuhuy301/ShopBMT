using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.DTOs.Product
{
    public class ProductVariantCreateDto
    {
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int Stock { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public IFormFile? ImageFile { get; set; } // ảnh riêng cho màu này
    }
}
