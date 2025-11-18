namespace backend_shopcaulong.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string? Size { get; set; } // ví dụ "39", "40", "41" hoặc "S, M, L"
        public string? Color { get; set; } // nếu sản phẩm có màu khác nhau
        public int Stock { get; set; }
        public decimal? Price { get; set; } // nếu biến thể có giá khác
        public decimal? DiscountPrice { get; set; }

        // Lịch sử tồn kho riêng cho biến thể
        public ICollection<StockHistory> StockHistories { get; set; }
    }
}
