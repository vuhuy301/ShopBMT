namespace backend_shopcaulong.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Màu của sản phẩm
        public int ColorVariantId { get; set; }
        public ProductColorVariant ColorVariant { get; set; }

        // Size của sản phẩm
        public int SizeVariantId { get; set; }
        public ProductSizeVariant SizeVariant { get; set; }

        // Số lượng mua
        public int Quantity { get; set; }

        // Giá tại thời điểm mua
        public decimal Price { get; set; }

        // Tổng tiền item
        public decimal Total => Quantity * Price;
    }

}
