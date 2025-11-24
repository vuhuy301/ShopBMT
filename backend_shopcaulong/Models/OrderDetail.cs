namespace backend_shopcaulong.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Số lượng sản phẩm
        public int Quantity { get; set; }

        // Giá tại thời điểm mua (không lấy giá từ bảng Product)
        public decimal Price { get; set; }
        public int? VariantId { get; set; }
        public ProductVariant? Variant { get; set; }


        // Tổng tiền item
        public decimal Total => Quantity * Price;
    }
}
