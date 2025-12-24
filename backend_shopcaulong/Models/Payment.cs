namespace backend_shopcaulong.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string Provider { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string? TransactionCode { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime? PaidAt { get; set; }

        public string? RawResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
