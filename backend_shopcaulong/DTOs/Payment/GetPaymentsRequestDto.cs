namespace backend_shopcaulong.DTOs.Payment
{
    public class GetPaymentsRequestDto
    {
        public string? Status { get; set; }           // Lọc theo trạng thái: Pending, Completed, Failed
        public string? Provider { get; set; }         // Lọc theo nhà cung cấp: VNPay, Momo, COD,...
        public int? OrderId { get; set; }             // Lọc theo ID đơn hàng
        public DateTime? FromDate { get; set; }       // Từ ngày thanh toán
        public DateTime? ToDate { get; set; }         // Đến ngày
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}