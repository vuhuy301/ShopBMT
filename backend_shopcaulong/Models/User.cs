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
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();

        // Reset Password token và thời gian hết hạn lưu trực tiếp trong User
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordTokenExpiry { get; set; }

        public string? GoogleId { get; set; }
        // public string? Avatar { get; set; }
        public bool EmailVerified { get; set; } = false;
    }
}
