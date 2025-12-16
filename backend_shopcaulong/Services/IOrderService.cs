using backend_shopcaulong.DTOs.Order;

namespace backend_shopcaulong.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, int? userId = null);

        // Mới: Get all orders cho admin
        Task<List<OrderDto>> GetAllOrdersAsync(GetOrdersRequest request);

        // Mới: Get my orders cho user
        Task<List<OrderDto>> GetMyOrdersAsync(int userId, GetOrdersRequest request);

        Task<OrderDto> UpdateOrderStatusAsync(int orderId, string newStatus, int adminUserId);

        Task<OrderDto?> GetOrderBySearchAsync(int? orderId = null, string? phone = null);
    }
}