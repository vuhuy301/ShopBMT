// Controllers/AdminDashboardController.cs
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaolong.Controllers
{
    /// <summary>
    /// API Dashboard dành cho Admin.
    /// </summary>
    [ApiController]
    [Route("api/admin/dashboard")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AdminDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Lấy doanh thu của hôm nay.
        /// </summary>
        [HttpGet("revenue-today")]
        public async Task<IActionResult> GetRevenueToday()
            => Ok(await _dashboardService.GetRevenueTodayAsync());

        /// <summary>
        /// Lấy doanh thu 7 ngày gần nhất.
        /// </summary>
        [HttpGet("revenue-last7days")]
        public async Task<IActionResult> GetRevenueLast7Days()
            => Ok(await _dashboardService.GetRevenueLast7DaysAsync());

        /// <summary>
        /// Lấy thống kê tổng quan đơn hàng.
        /// </summary>
        [HttpGet("orders-summary")]
        public async Task<IActionResult> GetOrdersSummary()
            => Ok(await _dashboardService.GetOrdersSummaryAsync());

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy.
        /// </summary>
        /// <param name="top">Số lượng sản phẩm muốn lấy.</param>
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int top = 10)
            => Ok(await _dashboardService.GetTopProductsAsync(top));

        /// <summary>
        /// Lấy danh sách sản phẩm sắp hết hàng.
        /// </summary>
        /// <param name="threshold">Ngưỡng tồn kho thấp.</param>
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
            => Ok(await _dashboardService.GetLowStockAsync(threshold));

        /// <summary>
        /// Lấy danh sách đơn hàng mới nhất.
        /// </summary>
        /// <param name="take">Số lượng đơn muốn lấy.</param>
        [HttpGet("latest-orders")]
        public async Task<IActionResult> GetLatestOrders([FromQuery] int take = 5)
            => Ok(await _dashboardService.GetLatestOrdersAsync(take));

        /// <summary>
        /// Lấy tỷ lệ hủy đơn hàng.
        /// </summary>
        [HttpGet("cancel-rate")]
        public async Task<IActionResult> GetCancelRate()
            => Ok(await _dashboardService.GetCancelRateAsync());
    }
}

}