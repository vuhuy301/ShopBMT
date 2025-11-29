namespace backend_shopcaulong.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal? DiscountPrice { get; set; }
        
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public bool IsFeatured { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public List<ProductImageDto> Images { get; set; } = new();
        public List<ProductDetailDto> Details { get; set; } = new();
        public List<ColorVariantDto> ColorVariants { get; set; } = new();
    }
}
