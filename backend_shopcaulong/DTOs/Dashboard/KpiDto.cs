namespace backend_shopcaulong.DTOs.Dashboard
{
    public class KpiDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalProfit { get; set; }
        public double? GrowthRate { get; set; }
    }
}
