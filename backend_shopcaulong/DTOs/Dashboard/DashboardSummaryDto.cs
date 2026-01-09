namespace backend_shopcaulong.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
    }
}
