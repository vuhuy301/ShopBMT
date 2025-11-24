using backend_shopcaulong.DTOs.User;
using System.Threading.Tasks;

namespace backend_shopcaulong.Services{
    public interface IUserService
        {
            Task<UserDto> RegisterAsync(RegisterDto dto);
            Task<UserDto?> AuthenticateAsync(string email, string password);
        }
}
