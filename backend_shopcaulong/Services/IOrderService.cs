
using backend_shopcaulong.DTOs.Order;

namespace backend_shopcaulong.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, int? userId = null);
    }
}