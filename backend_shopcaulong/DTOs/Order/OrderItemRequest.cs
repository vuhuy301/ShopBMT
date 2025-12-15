namespace backend_shopcaulong.DTOs.Order
{
   public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public string Color { get; set; }         // tên màu để frontend gửi dễ dàng
        public int ColorVariantId { get; set; }
        public string Size { get; set; }          // tên size
        public int SizeVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }        // giá tại thời điểm mua
    }
}