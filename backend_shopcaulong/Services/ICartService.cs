using backend_shopcaulong.DTOs.Cart;

namespace backend_shopcaulong.Services
{
    public interface ICartService
    {
        Task<StockCheckResult> CheckStockAsync(List<CartItemDto> cartItems);
    }
}
