namespace backend_shopcaulong.Models
{
    public class ProductColorVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string Color { get; set; }          // màu
        public ICollection<ProductImage> Images { get; set; } // ảnh theo màu
        public ICollection<ProductSizeVariant> Sizes { get; set; } // size
    }
}
