namespace backend_shopcaulong.DTOs.Order
{
    public class GetOrdersRequest
    {
        public int? OrderId { get; set; }     // Search theo mã đơn
        public string? Phone { get; set; }    // Search theo SĐT
        public string? Status { get; set; }   // Filter trạng thái

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}