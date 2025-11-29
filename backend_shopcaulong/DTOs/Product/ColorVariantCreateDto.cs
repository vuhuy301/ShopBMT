namespace backend_shopcaulong.DTOs.Product
{
    public class ColorVariantCreateDto
{
    public string Color { get; set; }
    public List<IFormFile>? ImageFiles { get; set; }
    public List<SizeVariantCreateDto> Sizes { get; set; } = new();
}
}