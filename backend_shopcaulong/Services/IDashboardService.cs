// Services/IDashboardService.cs
using backend_shopcaulong.DTOs.Dashboard;

namespace backend_shopcaulong.Services
{
    public interface IDashboardService
    {
        Task<decimal> GetRevenueTodayAsync();
        Task<List<RevenueLast7DaysDto>> GetRevenueLast7DaysAsync();
        Task<OrdersSummaryDto> GetOrdersSummaryAsync();
        Task<List<TopProductDto>> GetTopProductsAsync(int top = 10);
        Task<List<LowStockItemDto>> GetLowStockAsync(int threshold = 5);
        Task<List<LatestOrderDto>> GetLatestOrdersAsync(int take = 5);
        Task<CancelRateDto> GetCancelRateAsync();
    }
}