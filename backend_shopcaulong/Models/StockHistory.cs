namespace backend_shopcaulong.Models
{
    public class StockHistory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int? ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public int Change { get; set; } // +10 nhập kho, -3 bán hàng
        public string ActionType { get; set; } // Import, Sell, Adjust
        public DateTime CreatedAt { get; set; }
    }
}
