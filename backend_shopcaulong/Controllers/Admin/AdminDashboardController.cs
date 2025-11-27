// Controllers/AdminDashboardController.cs
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    // [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/dashboard")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public AdminDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("revenue-today")]
        public async Task<IActionResult> GetRevenueToday()
            => Ok(await _dashboardService.GetRevenueTodayAsync());

        [HttpGet("revenue-last7days")]
        public async Task<IActionResult> GetRevenueLast7Days()
            => Ok(await _dashboardService.GetRevenueLast7DaysAsync());

        [HttpGet("orders-summary")]
        public async Task<IActionResult> GetOrdersSummary()
            => Ok(await _dashboardService.GetOrdersSummaryAsync());

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int top = 10)
            => Ok(await _dashboardService.GetTopProductsAsync(top));

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
            => Ok(await _dashboardService.GetLowStockAsync(threshold));

        [HttpGet("latest-orders")]
        public async Task<IActionResult> GetLatestOrders([FromQuery] int take = 5)
            => Ok(await _dashboardService.GetLatestOrdersAsync(take));

        [HttpGet("cancel-rate")]
        public async Task<IActionResult> GetCancelRate()
            => Ok(await _dashboardService.GetCancelRateAsync());
    }
}