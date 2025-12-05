namespace backend_shopcaulong.Models
{
    public class ProductColorVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Color { get; set; } = null!;

        // ĐÃ CÓ – TỐT
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public ICollection<ProductSizeVariant> Sizes { get; set; } = new List<ProductSizeVariant>();
    }
}