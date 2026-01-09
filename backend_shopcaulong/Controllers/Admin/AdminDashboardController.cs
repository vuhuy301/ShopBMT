using backend_shopcaulong.Services;using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("kpi")]
        public async Task<IActionResult> GetKpi([FromQuery] int year, [FromQuery] int? month)
        {
            try
            {
                var kpi = await _dashboardService.GetKpiAsync(year, month);
                return Ok(kpi);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("revenue-by-days")]
        public async Task<IActionResult> GetRevenueByDays([FromQuery] int days = 30)
        {
            try
            {
                var data = await _dashboardService.GetRevenueByDaysAsync(days);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] int top = 5)
        {
            try
            {
                var data = await _dashboardService.GetTopProductsAsync(top);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    }
