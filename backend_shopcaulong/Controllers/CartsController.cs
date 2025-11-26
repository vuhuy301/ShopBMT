using AutoMapper;
using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace backend_shopcaulong.Controllers
{
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

        // Lấy giỏ hàng của user
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int userId = GetCurrentUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }
        // [HttpGet("{cartItemsId}")]
        // public async Task<IActionResult> GetCartByCartItemsId(int cartItemsId)
        // {
        //     int userId = GetCurrentUserId();
        //     await _cartService.RemoveItemAsync(userId, cartItemsId);
        //     return Ok(new { message = "Removed" });
        // }
        // Thêm item
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartAddItemDto dto)
        {
            int userId = GetCurrentUserId();
            await _cartService.AddItemAsync(userId, dto);
            return Ok(new { message = "Added to cart" });
        }

        // Cập nhật số lượng
        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] CartUpdateItemDto dto)
        {
            int userId = GetCurrentUserId();
            await _cartService.UpdateItemAsync(userId, dto);
            return Ok(new { message = "Updated" });
        }

        // Xóa item
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            int userId = GetCurrentUserId();
            await _cartService.RemoveItemAsync(userId, itemId);
            return Ok(new { message = "Removed" });
        }

        // Tích chọn / bỏ chọn item
        [HttpPut("toggle-select/{itemId}")]
        public async Task<IActionResult> ToggleSelect(int itemId)
        {
            int userId = GetCurrentUserId();
            await _cartService.ToggleSelectAsync(userId, itemId);
            return Ok(new { message = "Toggled select" });
        }

        // CHECKOUT
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto dto)
        {
            int userId = GetCurrentUserId();
            var result = await _cartService.CheckoutAsync(userId, dto);
            return Ok(result);
        }
        // LẤY TẤT CẢ ĐƠN HÀNG CỦA TÔI
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetCurrentUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        // XEM CHI TIẾT 1 ĐƠN HÀNG CỦA TÔI
        [HttpGet("my-orders/{orderId}")]
        public async Task<IActionResult> GetMyOrderDetail(int orderId)
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.GetOrderByIdForUserAsync(orderId, userId);

            if (order == null)
                return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền xem");

            return Ok(order);
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
