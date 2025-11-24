using Microsoft.AspNetCore.Mvc;
using backend_shopcaulong.DTOs.User;
using backend_shopcaulong.Services;

namespace backend_shopcaulong.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ShopDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(
            IUserService userService,
            JwtTokenService jwtTokenService,
            ShopDbContext context,
            IMapper mapper)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _context = context;
            _mapper = mapper;
        }


                [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _userService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
            if (token == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            return Ok(new { accessToken = token });
        }

        // GET: api/users/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT: api/users/profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var updatedUser = await _userService.UpdateProfileAsync(userId, dto);
            if (updatedUser == null) return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = dto.FullName;
            user.Phone = dto.Phone;
            user.Address = dto.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var oldPasswordHash = _userService.HashPassword(dto.OldPassword); // Cần public hoặc làm method helper
            if (user.PasswordHash != oldPasswordHash)
                return BadRequest(new { message = "Mật khẩu cũ không đúng" });

            user.PasswordHash = _userService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return NotFound();

            var token = GenerateResetToken(); // Bạn tự tạo token random hoặc GUID
            // Lưu token vào DB, gửi email

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            // Xác thực token và lấy userId
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null) return NotFound();

            user.PasswordHash = _userService.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

            if (payload == null)
                return BadRequest("Token Google không hợp lệ");

            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    FullName = payload.Name,
                    RoleId = 2,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var userDto = _mapper.Map<UserDto>(user);
            var token = _jwtTokenService.GenerateToken(userDto);

            return Ok(new { accessToken = token });
        }

        // Phần private helper bạn có thể thêm ở đây nếu cần
        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString("N"); // ví dụ
        }
    }
}
