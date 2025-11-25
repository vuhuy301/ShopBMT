namespace backend_shopcaulong.DTOs.User
{
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  // Lưu mật khẩu thô ở đây, sẽ hash trong service
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

}