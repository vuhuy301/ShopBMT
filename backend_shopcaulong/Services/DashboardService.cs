// Services/DashboardService.cs
using AutoMapper;
using backend_shopcaulong.DTOs.Dashboard;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ShopDbContext _db;
        private readonly IMapper _mapper;

        public DashboardService(ShopDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // DOANH THU HÔM NAY – ĐÃ FIX LỖI SUM(SUM)
        public async Task<decimal> GetRevenueTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _db.OrderDetails
                .Where(od => od.Order.CreatedAt >= today
                          && od.Order.CreatedAt < tomorrow
                          && od.Order.Status != "Cancelled")
                .SumAsync(od => od.Price * od.Quantity);
        }

        // DOANH THU 7 NGÀY GẦN NHẤT – ĐÃ FIX HOÀN TOÀN
        public async Task<List<RevenueLast7DaysDto>> GetRevenueLast7DaysAsync()
        {
            var result = new List<RevenueLast7DaysDto>();

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var nextDate = date.AddDays(1);

                var revenue = await _db.OrderDetails
                    .Where(od => od.Order.CreatedAt >= date
                              && od.Order.CreatedAt < nextDate
                              && od.Order.Status != "Cancelled")
                    .SumAsync(od => (decimal?)od.Price * od.Quantity) ?? 0m;

                result.Add(new RevenueLast7DaysDto
                {
                    Date = date.ToString("dd/MM"),
                    Revenue = revenue
                });
            }

            return result;
        }

        // TỔNG ĐƠN HÀNG + THEO TRẠNG THÁI
        public async Task<OrdersSummaryDto> GetOrdersSummaryAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var ordersToday = await _db.Orders
                .CountAsync(o => o.CreatedAt >= today && o.CreatedAt < tomorrow);

            var statusCounts = await _db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new StatusCountDto
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return new OrdersSummaryDto
            {
                OrdersToday = ordersToday,
                ByStatus = statusCounts
            };
        }

        // TOP SẢN PHẨM BÁN CHẠY – ĐÃ TỐI ƯU
        public async Task<List<TopProductDto>> GetTopProductsAsync(int top = 10)
        {
            var allowed = new[] { 5, 10, 15, 20 };
            if (!allowed.Contains(top)) top = 10;

            // Lấy dữ liệu ra trước → xử lý string trong C# → EF Core không cần dịch nữa
            var rawData = await _db.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Variant)
                .Select(od => new
                {
                    od.ProductId,
                    ProductName = od.Product.Name,
                    VariantColor = od.Variant != null ? od.Variant.Color : null,
                    VariantSize = od.Variant != null ? od.Variant.Size : null,
                    od.Quantity,
                    od.Price
                })
                .ToListAsync();

            var grouped = rawData
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.ProductName,
                    Variant = x.VariantColor != null && x.VariantSize != null
                        ? $"{x.VariantColor} {x.VariantSize}".Trim()
                        : (x.VariantColor ?? x.VariantSize ?? "Chính hãng")
                })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    Variant = g.Key.Variant,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Price * x.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(top)
                .ToList();

            return grouped;
        }

        // SẢN PHẨM SẮP HẾT HÀNG
        public async Task<List<LowStockItemDto>> GetLowStockAsync(int threshold = 5)
        {
            var products = await _db.Products
                .Where(p => p.Stock < threshold)
                .Select(p => new LowStockItemDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Variant = "",
                    Stock = p.Stock,
                    Warning = p.Stock <= 0 ? "HẾT HÀNG" : "SẮP HẾT"
                })
                .ToListAsync();

            var variants = await _db.ProductVariants
                .Where(v => v.Stock < threshold)
                .Include(v => v.Product)
                .Select(v => new LowStockItemDto
                {
                    ProductId = v.Product.Id,
                    ProductName = v.Product.Name,
                    Variant = $"{v.Color} {v.Size}".Trim(),
                    Stock = v.Stock,
                    Warning = v.Stock <= 0 ? "HẾT HÀNG" : "SẮP HẾT"
                })
                .ToListAsync();

            return products.Concat(variants)
                          .OrderBy(x => x.Stock)
                          .ThenBy(x => x.ProductName)
                          .ToList();
        }

        // ĐƠN HÀNG MỚI NHẤT
        public async Task<List<LatestOrderDto>> GetLatestOrdersAsync(int take = 5)
        {
            var allowed = new[] { 5, 10, 15, 20 };
            if (!allowed.Contains(take)) take = 5;

            return await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(take)
                .Select(o => new LatestOrderDto
                {
                    Id = o.Id,
                    Customer = o.User != null ? (o.User.FullName ?? o.Phone) : o.Phone,
                    TotalAmount = o.Items.Sum(i => i.Price * i.Quantity),
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        // TỶ LỆ HỦY ĐƠN
        public async Task<CancelRateDto> GetCancelRateAsync()
        {
            var total = await _db.Orders.CountAsync();
            var cancelled = await _db.Orders.CountAsync(o => o.Status == "Cancelled");
            var rate = total > 0 ? Math.Round((double)cancelled / total * 100, 2) : 0;

            return new CancelRateDto
            {
                TotalOrders = total,
                Cancelled = cancelled,
                CancelRate = $"{rate}%"
            };
        }
    }
}