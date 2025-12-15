namespace backend_shopcaulong.DTOs.Order
{
    public class CreateOrderResponse
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; } = "Đặt hàng thành công!";
    }
}
