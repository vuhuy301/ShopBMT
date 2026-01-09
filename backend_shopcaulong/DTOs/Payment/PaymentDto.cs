namespace backend_shopcaulong.DTOs.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? TransactionCode { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RawResponse { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}