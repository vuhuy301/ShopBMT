namespace backend_shopcaulong.DTOs.Cart
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Tổng tiền tạm tính
        public decimal TotalAmount { get; set; }

        // Danh sách item trong cart
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
