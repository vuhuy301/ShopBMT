namespace backend_shopcaulong.DTOs.User
{
    public class GetUsersRequestDto
    {
        public string? Email { get; set; }           // Lọc email (contains, chỉ khi >= 3 ký tự)
        public int? RoleId { get; set; }              // Thay RoleName bằng RoleId (null = tất cả)
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Optional: thêm lọc theo trạng thái active nếu cần sau này
        // public bool? IsActive { get; set; }
    }
}