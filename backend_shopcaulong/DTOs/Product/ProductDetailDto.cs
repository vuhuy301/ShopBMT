namespace backend_shopcaulong.DTOs.Product
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
    }
}
