using backend_shopcaulong.DTOs.Order;

namespace backend_shopcaulong.Services
{

    public interface IOrderService
{
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto?> GetOrderByIdAsync(int orderId);
    Task<bool> ApproveOrderAsync(int orderId);
    Task<bool> CancelOrderAsync(int orderId);
    Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus);
}
}