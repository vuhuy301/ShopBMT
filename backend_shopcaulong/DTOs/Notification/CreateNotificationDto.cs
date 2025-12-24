namespace backend_shopcaulong.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int? ReferenceId { get; set; }
    }
}