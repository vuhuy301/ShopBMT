using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend_shopcaulong.Services;
using backend_shopcaulong.DTOs.Order;

namespace backend_shopcaulong.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/orders/create
        [HttpPost("create")]
        // Không bắt buộc đăng nhập → AllowAnonymous
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lấy UserId nếu có đăng nhập, không thì để null (khách vãng lai)
                int? userId = GetCurrentUserIdOrNull();
                var response = await _orderService.CreateOrderAsync(request, userId);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy UserId từ token nếu có, không thì trả về null (khách vãng lai)
        /// </summary>
        private int? GetCurrentUserIdOrNull()
        {
            // Nếu không có token hoặc chưa auth → User.Identity.IsAuthenticated = false
            if (!User.Identity?.IsAuthenticated ?? true)
                return null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? User.FindFirst("sub")?.Value; // một số JWT dùng "sub" không có namespace

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null; // ID không phải số → coi như không hợp lệ, để null
        }
    }
}