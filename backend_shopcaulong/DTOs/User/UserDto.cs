namespace backend_shopcaulong.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!; // bắt buộc phải có giá trị

        public DateTime CreatedAt { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ProfileUpdatedAt { get; set; }
    }
}
