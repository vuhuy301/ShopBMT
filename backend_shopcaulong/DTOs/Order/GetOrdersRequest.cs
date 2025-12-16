namespace backend_shopcaulong.DTOs.Order
{
    public class GetOrdersRequest
    {
        // public int Page { get; set; } = 1;
        // public int PageSize { get; set; } = 10;
        public string? Status { get; set; } // Filter by status (e.g., "Pending")
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}