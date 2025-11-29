// DTOs/Product/SizeVariantDto.cs
namespace backend_shopcaulong.DTOs.Product
{
    public class SizeVariantDto
    {
        public int Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }

        // CẢ 2 ĐỀU LÀ decimal? ĐỂ TRÁNH LỖI ??
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        // Giờ dùng ?? thoải mái, không lỗi!
        public decimal FinalPrice => DiscountPrice ?? Price ?? 0;

        // Tiện cho frontend kiểm tra còn hàng
        public bool InStock => Stock > 0;
    }
}