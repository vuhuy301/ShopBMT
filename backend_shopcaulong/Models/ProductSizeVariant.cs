namespace backend_shopcaulong.Models
{
    public class ProductSizeVariant
    {
        public int Id { get; set; }
        public int ColorVariantId { get; set; }
        public ProductColorVariant ColorVariant { get; set; }

        public string Size { get; set; }
        public int Stock { get; set; }
        public decimal? Price { get; set; }
    }
}
