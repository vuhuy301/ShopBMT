using AutoMapper;
using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace backend_shopcaulong.Controllers
{
    /// <summary>
    /// API quản lý giỏ hàng và đơn hàng của người dùng.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        public CartsController(ICartService cartService, IOrderService orderService)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy giỏ hàng của người dùng hiện tại.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int userId = GetCurrentUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng.
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartAddItemDto dto)
        {
            int userId = GetCurrentUserId();
            await _cartService.AddItemAsync(userId, dto);
            return Ok(new { message = "Added to cart" });
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng.
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] CartUpdateItemDto dto)
        {
            int userId = GetCurrentUserId();
            await _cartService.UpdateItemAsync(userId, dto);
            return Ok(new { message = "Updated" });
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng.
        /// </summary>
        /// <param name="itemId">ID sản phẩm trong giỏ hàng.</param>
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            int userId = GetCurrentUserId();
            await _cartService.RemoveItemAsync(userId, itemId);
            return Ok(new { message = "Removed" });
        }

        /// <summary>
        /// Tích hoặc bỏ tích chọn sản phẩm trong giỏ hàng.
        /// </summary>
        /// <param name="itemId">ID sản phẩm trong giỏ hàng.</param>
        [HttpPut("toggle-select/{itemId}")]
        public async Task<IActionResult> ToggleSelect(int itemId)
        {
            int userId = GetCurrentUserId();
            await _cartService.ToggleSelectAsync(userId, itemId);
            return Ok(new { message = "Toggled select" });
        }

        // ====== Lấy userId từ JWT ======
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? throw new UnauthorizedAccessException("Không tìm thấy user ID trong token");

            if (!int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User ID không hợp lệ");

            return userId;
        }
    }
}
