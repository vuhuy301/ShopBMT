//// using Microsoft.EntityFrameworkCore;
//using System.Security.Cryptography;
//using System.Text;
//using backend_shopcaulong.Models;
//using backend_shopcaulong.DTOs.User;
//using backend_shopcaulong.Services;
//using AutoMapper;
//using Microsoft.EntityFrameworkCore;

//namespace backend_shopcaulong.Services
//{
//    public class UserService : IUserService
//    {
//        private readonly ShopDbContext _context;
//        private readonly IMapper _mapper;

//        public UserService(ShopDbContext context, IMapper mapper)
//        {
//            _context = context;
//            _mapper = mapper;
//        }

//        // Hash mật khẩu dùng SHA256 (có thể thay bằng BCrypt hoặc khác)
//        private string HashPassword(string password)
//        {
//            using var sha = SHA256.Create();
//            var bytes = Encoding.UTF8.GetBytes(password);
//            var hash = sha.ComputeHash(bytes);
//            return Convert.ToBase64String(hash);
//        }

//        // Đăng ký user mới, trả về UserDto
//        public async Task<UserDto> RegisterAsync(RegisterDto dto)
//        {
//            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
//                throw new Exception("Email đã được đăng ký");

//            var user = new User
//            {
//                FullName = dto.FullName,
//                Email = dto.Email,
//                PasswordHash = HashPassword(dto.Password),
//                Phone = dto.Phone,
//                Address = dto.Address,
//                CreatedAt = DateTime.Now,
//                RoleId = 2 // giả sử 2 là role user
//            };

//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();

//            // Load Role để map RoleName
//            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

//            return _mapper.Map<UserDto>(user);
//        }

//        // Xác thực đăng nhập, trả về UserDto hoặc null
//        public async Task<UserDto?> AuthenticateAsync(string email, string password)
//        {
//            var passwordHash = HashPassword(password);
//            var user = await _context.Users
//                .Include(u => u.Role)
//                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);

//            if (user == null)
//                return null;

//            return _mapper.Map<UserDto>(user);
//        }
//    }
//}
