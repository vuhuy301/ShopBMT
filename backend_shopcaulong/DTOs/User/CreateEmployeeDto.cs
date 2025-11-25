namespace backend_shopcaulong.DTOs.User
{
    public class CreateEmployeeDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Employee"; // Admin hoặc Employee
}
}