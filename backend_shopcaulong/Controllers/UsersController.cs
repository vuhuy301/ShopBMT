using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend_shopcaulong.DTOs.User;
using backend_shopcaulong.Services;
using Google.Apis.Auth;

namespace backend_shopcaulong.Controllers
{
    /// <summary>
    /// API quản lý người dùng (đăng ký, đăng nhập, thông tin cá nhân,...)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly JwtTokenService _jwtTokenService;   // thêm dòng này

        public UsersController(IUserService userService, JwtTokenService jwtTokenService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;             // thêm dòng này
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng (dành cho Admin).
        /// </summary>
        // [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Đăng ký tài khoản mới.
        /// </summary>
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
        /// <summary>
        /// Đăng nhập với email và mật khẩu, trả về JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
            if (token == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            return Ok(new { accessToken = token });
        }

        /// <summary>
        /// Lấy thông tin hồ sơ người dùng hiện tại.
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        /// <summary>
        /// Cập nhật thông tin hồ sơ người dùng hiện tại.
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = GetCurrentUserId();;
            var updatedUser = await _userService.UpdateProfileAsync(userId, dto);
            if (updatedUser == null) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Đổi mật khẩu người dùng hiện tại.
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _userService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            if (!result)
                return BadRequest(new { message = "Mật khẩu cũ không đúng hoặc người dùng không tồn tại" });

            return NoContent();
        }

        /// <summary>
        /// Gửi email yêu cầu đặt lại mật khẩu.
        /// </summary>
        [HttpPost("reset-password-request")]
        public async Task<IActionResult> ResetPasswordRequest([FromBody] ResetPasswordRequestDto dto)
        {
            var result = await _userService.SendResetPasswordEmailAsync(dto.Email);
            if (!result) return NotFound(new { message = "Email không tồn tại" });

            return Ok(new { message = "Email đổi mật khẩu đã được gửi (nếu email tồn tại)" });
        }
        /// <summary>
        /// Đặt lại mật khẩu bằng token được gửi qua email.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _userService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result) return BadRequest(new { message = "Token không hợp lệ hoặc đã hết hạn" });

            return NoContent();
        }

        /// <summary>
        /// Đăng nhập bằng Google OAuth, trả về JWT token và thông tin người dùng.
        /// </summary>
        [HttpPost("google-login")]
        public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token);

                var userDto = await _userService.UpsertGoogleUserAsync(
                    googleId: payload.Subject,
                    email: payload.Email,
                    fullName: payload.Name ?? payload.Email.Split('@')[0]
                    // avatar: payload.Picture
                );

                var jwtToken = _jwtTokenService.GenerateToken(userDto);

                return Ok(new
                {
                    accessToken = jwtToken,
                    user = userDto
                });
            }
            catch (InvalidJwtException ex)
            {
                return BadRequest(new { message = "Token Google không hợp lệ", detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", detail = ex.Message });
            }
        }
        private int GetCurrentUserId()
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? throw new UnauthorizedAccessException("Không tìm thấy user ID trong token");

                if (!int.TryParse(userIdClaim, out var userId))
                    throw new UnauthorizedAccessException("User ID không hợp lệ");

                return userId;
            }
        
    }
    
}
