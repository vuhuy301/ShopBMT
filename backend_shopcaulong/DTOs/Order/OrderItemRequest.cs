namespace backend_shopcaulong.DTOs.Order
{
   public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int ColorVariantId { get; set; }
        public int SizeVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}