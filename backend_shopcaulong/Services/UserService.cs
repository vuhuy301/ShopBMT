using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using backend_shopcaulong.DTOs.User;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace backend_shopcaulong.Services
{
    public class UserService : IUserService
    {
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;

        public UserService(ShopDbContext context, IMapper mapper, JwtTokenService jwtTokenService, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _jwtTokenService = jwtTokenService;
            _configuration = configuration;
        }

        // Hash mật khẩu SHA256 (nên dùng BCrypt trong thực tế)
        public string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email đã được đăng ký");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Phone = dto.Phone,
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow,
                RoleId = 2
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<string?> AuthenticateAsync(string email, string password)
        {
            var passwordHash = HashPassword(password);
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);

            if (user == null)
                return null;

            var userDto = _mapper.Map<UserDto>(user);
            return _jwtTokenService.GenerateToken(userDto);
        }

        public async Task<UserDto?> GetByIdAsync(int userId)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.Address = dto.Address;

            await _context.SaveChangesAsync();

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var oldPasswordHash = HashPassword(oldPassword);
            if (user.PasswordHash != oldPasswordHash) return false;

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        // Gửi email reset password - lưu token + expiry trong User
        public async Task<bool> SendResetPasswordEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // Tạo token (random GUID)
            user.ResetPasswordToken = Guid.NewGuid().ToString("N");
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1); // token hiệu lực 1 giờ

            await _context.SaveChangesAsync();

            // TODO: Gửi mail có link reset kiểu: https://yourdomain.com/reset-password?token=xxxx

            return true;
        }

        // Reset password bằng token
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.ResetPasswordToken == token &&
                u.ResetPasswordTokenExpiry != null &&
                u.ResetPasswordTokenExpiry > DateTime.UtcNow);

            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);

            // Xóa token sau khi đổi
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FullName = u.FullName,
                        Email = u.Email,
                        RoleId = u.RoleId,
                        RoleName = u.Role.Name,  
                        CreatedAt = u.CreatedAt,
                        Phone = u.Phone,
                        Address = u.Address
                    })
                    .ToListAsync();

                return users;
            }


    }
}
