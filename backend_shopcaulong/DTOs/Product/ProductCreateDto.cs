using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.DTOs.Product
{
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public bool IsFeatured { get; set; } = false;

        [FromForm] public IFormFileCollection? ImageFiles { get; set; }
        [FromForm] public List<ProductDetailCreateDto> Details { get; set; } = new();
        [FromForm] public List<ProductVariantCreateDto> Variants { get; set; } = new();
    }
}
