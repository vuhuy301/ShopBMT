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
        private readonly IEmailSender _emailSender;

        public UserService(ShopDbContext context, IMapper mapper, JwtTokenService jwtTokenService, IConfiguration configuration, IEmailSender emailSender)
            {
                _context = context;
                _mapper = mapper;
                _jwtTokenService = jwtTokenService;
                _configuration = configuration;
                _emailSender = emailSender; // Đừng quên inject
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
            if (user == null) 
                return false; // Vẫn trả false để không lộ thông tin tài khoản tồn tại

            // Tạo token
            user.ResetPasswordToken = Guid.NewGuid().ToString("N");
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            // Link reset (thay bằng domain thật của bạn)
            var frontendUrl = _configuration["FrontendUrl"] ?? "https://shopcaulong.vercel.app";
            var resetLink = $"{frontendUrl}/reset-password?token={user.ResetPasswordToken}";

            // Nội dung email đẹp
            var subject = "Đặt lại mật khẩu tài khoản Shop Cầu Lông";
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #d4380d;'>Yêu cầu đặt lại mật khẩu</h2>
                    <p>Xin chào <strong>{user.FullName}</strong>,</p>
                    <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                    <p>Vui lòng nhấn vào nút bên dưới để đặt lại mật khẩu (link chỉ có hiệu lực trong <strong>1 giờ</strong>):</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #d4380d; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Đặt lại mật khẩu ngay
                        </a>
                    </div>
                    
                    <p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>
                    <hr>
                    <small>Shop Cầu Lông - Hệ thống bán vợt cầu lông uy tín</small>
                </div>";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, htmlMessage);
            }
            catch (Exception ex)
            {
                // Nên log lỗi ở đây (Serilog, NLog,...)
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                // Không throw để frontend không biết lỗi server
            }

            return true; // Luôn trả true để không lộ user tồn tại
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
        public async Task<UserDto> UpsertGoogleUserAsync(string googleId, string email, string fullName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == email);

            if (user == null)
            {
                // Tạo mới
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    EmailVerified = true,
                    GoogleId = googleId,
                    // Avatar = avatar,
                    PasswordHash = "", // không cần password
                    RoleId = 2, // Customer
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
            }
            else
            {
                // Cập nhật thông tin nếu đã tồn tại
                user.GoogleId ??= googleId;
                user.FullName = fullName;
                // user.Avatar = avatar ?? user.Avatar;
                user.EmailVerified = true;
            }

            await _context.SaveChangesAsync();
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            return _mapper.Map<UserDto>(user);
        }

    }
}
