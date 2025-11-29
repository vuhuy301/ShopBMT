namespace backend_shopcaulong.Models
{
    public class StockHistory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // ⭐ SizeVariant là đơn vị tồn kho thật
        public int? SizeVariantId { get; set; }
        public ProductSizeVariant SizeVariant { get; set; }

        // (Option) Nếu bạn muốn log thêm màu → cho dễ xem báo cáo
        public int? ColorVariantId { get; set; }
        public ProductColorVariant ColorVariant { get; set; }

        public int Change { get; set; } // +10 nhập kho, -3 bán hàng
        public string ActionType { get; set; } // Import, Sell, Adjust
        public DateTime CreatedAt { get; set; }
    }

}
