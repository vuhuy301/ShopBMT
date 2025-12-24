namespace backend_shopcaulong.DTOs.Order
{
    public class CreateOrderRequest
    {
        public string Name { get; set; } = string.Empty;         // Tên khách hàng
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? Email { get; set; }      // Ghi chú (tùy chọn)
        public string PaymentMethod { get; set; } = "cod";       // "cod" hoặc "bank" (sẽ map sang COD/Bank)

        public List<OrderItemRequest> Items { get; set; } = new();

    }
}