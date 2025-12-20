namespace backend_shopcaulong.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int? ReferenceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}