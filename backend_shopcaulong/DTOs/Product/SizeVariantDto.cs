// DTOs/Product/SizeVariantDto.cs
namespace backend_shopcaulong.DTOs.Product
{
    public class SizeVariantDto
    {
        public int Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }

        // Tiện cho frontend kiểm tra còn hàng
        public bool InStock => Stock > 0;
    }
}