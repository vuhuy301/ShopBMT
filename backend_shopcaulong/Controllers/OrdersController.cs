using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using backend_shopcaulong.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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

        // GET: api/orders/all
        // Chỉ Admin mới được xem tất cả đơn hàng
        [HttpGet("all")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] GetOrdersRequest request)
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(request);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/orders/my-orders
        // Người dùng đã đăng nhập xem đơn hàng của chính mình
        [HttpGet("my-orders")]
        [Authorize] // Bắt buộc phải đăng nhập
        public async Task<IActionResult> GetMyOrders([FromQuery] GetOrdersRequest request)
        {
            try
            {
                int userId = GetCurrentUserId(); // Ở đây bắt buộc có UserId vì đã [Authorize]
                var orders = await _orderService.GetMyOrdersAsync(userId, request);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/orders/create
        // Cho phép cả khách vãng lai và người đăng nhập
        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Lấy UserId nếu đã đăng nhập, không thì null (khách vãng lai)
                int? userId = GetCurrentUserIdOrNull();

                var response = await _orderService.CreateOrderAsync(request, userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // PUT: api/orders/{id}/status - Chỉ Admin
        [HttpPut("{id}/status")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                int adminUserId = GetCurrentUserId();

                var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, request.Status, adminUserId);

                return Ok(new 
                { 
                    message = "Cập nhật trạng thái đơn hàng thành công!", 
                    order = updatedOrder 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
       
        /// <summary>
        /// Lấy UserId từ JWT token nếu người dùng đã đăng nhập.
        /// Nếu không có token hoặc token không hợp lệ → trả về null (dùng cho khách vãng lai).
        /// </summary>
        private int? GetCurrentUserIdOrNull()
        {
            // Nếu không có token hoặc chưa xác thực → trả về null
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return null;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            return null; // UserId không phải số → không hợp lệ
        }

        /// <summary>
        /// Lấy UserId bắt buộc (ném exception nếu không có hoặc không hợp lệ).
        /// Dùng cho các endpoint yêu cầu đăng nhập như GetMyOrders.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                              ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Không tìm thấy User ID trong token.");

            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User ID trong token không hợp lệ.");

            return userId;
        }
    }
}