using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.DTOs.Order;

namespace backend_shopcaulong.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task AddItemAsync(int userId, CartAddItemDto dto);
        Task UpdateItemAsync(int userId, CartUpdateItemDto dto);
        Task RemoveItemAsync(int userId, int cartItemId);
        Task ToggleSelectAsync(int userId, int cartItemId);
    }

}
