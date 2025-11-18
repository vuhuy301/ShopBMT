namespace backend_shopcaulong.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // FK Role
        public int RoleId { get; set; }
        public Role Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Nếu bạn muốn lưu địa chỉ, số điện thoại…
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
