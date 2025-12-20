using Microsoft.AspNetCore.Mvc;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using backend_shopcaulong.DTOs.Notification;

namespace backend_shopcaulong.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "Admin,Staff")] // Chỉ admin/staff mới xem được
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContext; // để lấy UserId hiện tại

        public NotificationsController(INotificationService notificationService, IHttpContextAccessor httpContext)
        {
            _notificationService = notificationService;
            _httpContext = httpContext;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // GET: api/notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications()
        {
            var notifications = await _notificationService.GetByUserIdAsync(CurrentUserId);
            return Ok(notifications);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var count = await _notificationService.GetUnreadCountAsync(CurrentUserId);
            return Ok(count);
        }

        // PATCH: api/notifications/5/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id, CurrentUserId);
            return NoContent();
        }
    }
}