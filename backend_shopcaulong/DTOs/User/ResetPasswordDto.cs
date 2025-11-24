namespace backend_shopcaulong.DTOs.User {
    public class ResetPasswordDto
{
    public int UserId { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
}