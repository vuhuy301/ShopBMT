using Microsoft.EntityFrameworkCore;
using backend_shopcaulong.DTOs.Notification;
using backend_shopcaulong.Models;

namespace backend_shopcaulong.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ShopDbContext _context;

        public NotificationService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NotificationDto>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    ReferenceId = n.ReferenceId,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<NotificationDto> CreateAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                ReferenceId = dto.ReferenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                ReferenceId = notification.ReferenceId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}