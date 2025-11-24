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

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = await _userService.RegisterAsync(registerDto);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 400 nếu có email đã tồn tại hoặc lỗi khác
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.AuthenticateAsync(loginDto.Email, loginDto.Password);
            if (user == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
            }

            // Nếu có JWT, trả token ở đây, hiện chỉ trả userDto thôi
            return Ok(user);
        }
    }
}
