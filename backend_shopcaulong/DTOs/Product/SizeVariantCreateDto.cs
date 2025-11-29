namespace backend_shopcaulong.DTOs.Product
{
    public class SizeVariantCreateDto
{
    public string Size { get; set; }
    public int Stock { get; set; }
    public decimal? Price { get; set; } // có thể khác giá chung
}
}