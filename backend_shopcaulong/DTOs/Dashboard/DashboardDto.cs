// backend_shopcaulong/DTOs/Dashboard/DashboardDto.cs
namespace backend_shopcaulong.DTOs.Dashboard
{
    public class RevenueTodayDto
    {
        public decimal RevenueToday { get; set; }
    }

    public class RevenueLast7DaysDto
    {
        public string Date { get; set; } = null!;
        public decimal Revenue { get; set; }
    }

    public class OrdersSummaryDto
    {
        public int OrdersToday { get; set; }
        public List<StatusCountDto> ByStatus { get; set; } = new();
    }

    public class StatusCountDto
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Variant { get; set; } = "";
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string Variant { get; set; } = "";
        public int Stock { get; set; }
        public string Warning { get; set; } = null!;
    }

    public class LatestOrderDto
    {
        public int Id { get; set; }
        public string Customer { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CancelRateDto
    {
        public int TotalOrders { get; set; }
        public int Cancelled { get; set; }
        public string CancelRate { get; set; } = null!;
    }
}