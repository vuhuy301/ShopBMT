namespace backend_shopcaulong.DTOs.Product
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int Stock { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
    }
}
