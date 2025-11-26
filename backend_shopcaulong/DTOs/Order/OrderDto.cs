namespace backend_shopcaulong.DTOs.Order
{
    
    public class OrderDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserFullName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string Phone { get; set; }
        public List<OrderDetailDto> Items { get; set; }
    }
}