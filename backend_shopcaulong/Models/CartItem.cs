namespace backend_shopcaulong.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }

        public int Quantity { get; set; }

        // Price lưu giá tại thời điểm thêm vào giỏ (tùy bạn chọn lưu hay lấy lại lúc Checkout)
        public decimal Price { get; set; }

        // Selected flag — nếu FE muốn tích chọn các item để mua
        public bool Selected { get; set; } = true;
    }

}
