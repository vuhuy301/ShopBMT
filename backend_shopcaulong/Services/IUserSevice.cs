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

        Task<bool> SendResetPasswordEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpsertGoogleUserAsync(string googleId, string email, string fullName);
        //Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto);
        //Task<bool> UpdateUserRoleAsync(int userId, string newRole);
        //Task<bool> DeleteUserAsync(int userId);

    }
}
