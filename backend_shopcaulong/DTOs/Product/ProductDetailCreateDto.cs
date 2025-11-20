namespace backend_shopcaulong.DTOs.Product
{
    public class ProductDetailCreateDto
    {
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
    }
}
