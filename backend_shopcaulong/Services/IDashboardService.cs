// Services/IDashboardService.cs
using backend_shopcaulong.DTOs.Dashboard;

namespace backend_shopcaulong.Services
{
    public interface IDashboardService
    {
        Task<KpiDto> GetKpiAsync(int year, int? month = null);
        Task<List<RevenueByDateDto>> GetRevenueByDaysAsync(int days = 30);
        Task<List<TopProductDto>> GetTopProductsAsync(int top = 5);
    }
}