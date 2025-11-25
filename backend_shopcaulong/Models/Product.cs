namespace backend_shopcaulong.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string? Description { get; set; }
        public decimal Price { get; set; }

        public int Stock { get; set; }
        public decimal? DiscountPrice { get; set; }

        // Thương hiệu
        public int BrandId { get; set; }
        public Brand Brand { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<ProductPromotion> ProductPromotions { get; set; }

        // 1 sp nhiều ảnh
        public ICollection<ProductImage> Images { get; set; }

        // 1 sp nhiều đoạn mô tả
        public ICollection<ProductDetail> Details { get; set; }

        public bool IsFeatured { get; set; }
        public ICollection<StockHistory> StockHistories { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    }
}
