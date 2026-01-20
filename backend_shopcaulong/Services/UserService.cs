using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using backend_shopcaulong.DTOs.Common;
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
                RoleId = 3,
                IsActive = true
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
            
            if (!user.IsActive)
                throw new Exception("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên.");

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
            
            user.ProfileUpdatedAt = DateTime.UtcNow;
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
                        Address = u.Address,
                        IsActive = u.IsActive,
                        ProfileUpdatedAt = u.ProfileUpdatedAt
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
        public async Task<UserDto> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email đã tồn tại!");

            // Lấy role từ DB
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (role == null)
                throw new Exception("Role không hợp lệ! Chỉ Admin hoặc Employee");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Phone = "",
                Address = "",
                CreatedAt = DateTime.UtcNow,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            return _mapper.Map<UserDto>(user);
        }
        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Lấy RoleId từ role name
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == newRole);
            if (role == null)
                throw new Exception("Role không tồn tại");

            user.RoleId = role.Id;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ToggleUserActiveStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
                return false;

            // Lưu trạng thái cũ để kiểm tra xem có phải "vừa bị khóa" không
            bool previousStatus = user.IsActive;

            // Cập nhật trạng thái mới
            user.IsActive = isActive;

            await _context.SaveChangesAsync();

            // Chỉ gửi email khi: Trước đó đang ACTIVE mà bây giờ chuyển sang INACTIVE (tức là vừa bị khóa)
            if (previousStatus && !isActive)
            {
                try
                {
                    var subject = "Tài khoản của bạn đã bị tạm khóa";

                    var htmlMessage = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #f9f9f9;'>
                            <h2 style='color: #d4380d;'>Thông báo từ Shop Cầu Lông</h2>
                            <p>Xin chào <strong>{user.FullName}</strong>,</p>
                            <p>Chúng tôi rất tiếc phải thông báo rằng tài khoản của bạn đã bị <strong>tạm khóa</strong> do vi phạm một hoặc nhiều quy định sử dụng dịch vụ.</p>
                            <p>Nếu bạn cho rằng đây là nhầm lẫn hoặc cần giải thích thêm, vui lòng liên hệ với chúng tôi qua:</p>
                            <ul>
                                <li>Email hỗ trợ: <a href='mailto:support@shopcaulong.com'>support@shopcaulong.com</a></li>
                                <li>Fanpage: <a href='https://facebook.com/shopcaulong' target='_blank'>facebook.com/shopcaulong</a></li>
                            </ul>
                            <p>Chúng tôi sẽ xem xét và phản hồi trong thời gian sớm nhất.</p>
                            <p>Trân trọng,<br>Đội ngũ Shop Cầu Lông</p>
                            <hr>
                            <small>Đây là email tự động, vui lòng không trả lời trực tiếp email này.</small>
                        </div>";

                    await _emailSender.SendEmailAsync(user.Email, subject, htmlMessage);
                }
                catch (Exception ex)
                {
                    // Không throw lỗi để không làm hỏng flow chính (khóa tài khoản vẫn thành công)
                    Console.WriteLine($"Lỗi khi gửi email thông báo khóa tài khoản cho user {user.Id}: {ex.Message}");
                    // Nếu bạn dùng Serilog, NLog hoặc ILogger thì nên log đúng cách ở đây
                }
            }

            return true;
        }

        public async Task<PagedResultDto<UserDto>> GetUsersPagedAsync(GetUsersRequestDto request)
        {
            // Validate phân trang
            if (request.PageSize > 100) request.PageSize = 100;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageNumber < 1) request.PageNumber = 1;

            var query = _context.Users
                .Include(u => u.Role)
                .AsQueryable();

            // Lọc Email (chỉ khi >= 3 ký tự)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var emailSearch = request.Email.Trim();
                if (emailSearch.Length >= 3)
                {
                    var emailLower = emailSearch.ToLower();
                    query = query.Where(u => u.Email.ToLower().Contains(emailLower));
                }
            }

            // Lọc theo RoleId
            if (request.RoleId.HasValue)
            {
                query = query.Where(u => u.RoleId == request.RoleId.Value);
            }

            // Đếm tổng số bản ghi sau lọc
            int totalItems = await query.CountAsync();

            // Tính tổng số trang
            int totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            // Lấy dữ liệu phân trang
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleId = u.RoleId,
                    RoleName = u.Role.Name,
                    CreatedAt = u.CreatedAt,
                    Phone = u.Phone,
                    Address = u.Address,
                    IsActive = u.IsActive,
                    ProfileUpdatedAt = u.ProfileUpdatedAt
                })
                .ToListAsync();

            // Trả về theo đúng format bạn đã có
            return new PagedResultDto<UserDto>
            {
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = users
            };
        }
    }
}
