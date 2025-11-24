using System.Threading.Tasks;
using backend_shopcaulong.DTOs.User;

namespace backend_shopcaulong.Services
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<string?> AuthenticateAsync(string email, string password);
        Task<UserDto?> GetByIdAsync(int userId);
        Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        // Nếu có refresh token, thêm logout như:
        // Task LogoutAsync(int userId);
    }
}
