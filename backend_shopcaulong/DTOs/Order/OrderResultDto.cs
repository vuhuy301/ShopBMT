namespace backend_shopcaulong.DTOs.Order
{
    public class OrderResultDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
