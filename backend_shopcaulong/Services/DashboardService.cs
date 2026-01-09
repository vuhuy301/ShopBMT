using backend_shopcaulong.DTOs.Dashboard;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ShopDbContext _context;

        public DashboardService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<KpiDto> GetKpiAsync(int year, int? month = null)
        {
            var start = month.HasValue
                ? new DateTime(year, month.Value, 1)
                : new DateTime(year, 1, 1);
            var end = month.HasValue
                ? start.AddMonths(1)
                : start.AddYears(1);

            var ordersQuery = _context.Orders
                .Where(o => (o.Status == "Đã thanh toán" || o.Status == "Hoàn thành") &&
                            o.CreatedAt >= start && o.CreatedAt < end);

            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            var totalOrders = await ordersQuery.CountAsync();
            var totalProfit = await ordersQuery
    .SelectMany(o => o.Items)
    .SumAsync(od => (int?)od.Quantity) ?? 0;

            return new KpiDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProfit = totalProfit
            };
        }

        /// <summary>
        /// Doanh thu theo ngày trong n ngày gần nhất
        /// </summary>
        public async Task<List<RevenueByDateDto>> GetRevenueByDaysAsync(int days = 30)
        {
            var start = DateTime.Today.AddDays(-days + 1);

            var data = await _context.Orders
                .Where(o => (o.Status == "Đã thanh toán" || o.Status == "Hoàn thành") && o.CreatedAt >= start)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new RevenueByDateDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .ToListAsync();

            // Fill các ngày không có doanh thu
            var result = Enumerable.Range(0, days)
                .Select(i =>
                {
                    var date = start.AddDays(i);
                    var revenue = data.FirstOrDefault(d => d.Date == date)?.Revenue ?? 0;
                    return new RevenueByDateDto
                    {
                        Date = date,
                        Revenue = revenue
                    };
                }).ToList();

            return result;
        }

        /// <summary>
        /// Top sản phẩm bán chạy theo số lượng
        /// </summary>
        public async Task<List<TopProductDto>> GetTopProductsAsync(int top = 5)
        {
            return await _context.OrderDetails
                .Where(od => od.Order.Status == "Đã thanh toán" || od.Order.Status == "Hoàn thành")
                .GroupBy(od => new { od.ProductId, od.Product.Name })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToListAsync();
        }
    }

}

