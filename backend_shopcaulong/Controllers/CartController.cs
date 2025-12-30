using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("check-stock")]
        public async Task<IActionResult> CheckStock([FromBody] List<CartItemDto> cartItems)
        {
            var result = await _cartService.CheckStockAsync(cartItems);

            if (result.Ok)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
