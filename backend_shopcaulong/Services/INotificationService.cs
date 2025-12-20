using backend_shopcaulong.DTOs.Notification;

namespace backend_shopcaulong.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId);
        Task<NotificationDto> CreateAsync(CreateNotificationDto dto);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}