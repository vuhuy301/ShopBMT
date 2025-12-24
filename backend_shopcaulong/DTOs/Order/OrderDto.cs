namespace backend_shopcaulong.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? Email { get; set; }
        public List<OrderDetailDto> Items { get; set; } = new();
    }
}