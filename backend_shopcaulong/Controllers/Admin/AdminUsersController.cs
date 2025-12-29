using backend_shopcaulong.DTOs.Common;
using backend_shopcaulong.DTOs.User;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    // [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminUsersController(IUserService userService)
        {
            _userService = userService;
        }

        // Lấy danh sách người dùng
        /// <summary>
        /// Lấy tất cả người dùng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

         // Lấy danh sách người dùng theo filters
        /// <summary>
        /// Lấy tất cả người dùng theo filters
        /// </summary>
        [HttpGet("users")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsersPaged([FromQuery] GetUsersRequestDto request)
        {
            var users = await _userService.GetUsersPagedAsync(request);
            return Ok(users);
        }

        // Lấy người dùng theo ID
        /// <summary>
        /// Lấy người dùng theo ID.
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // Tạo nhân viên
        /// <summary>
        /// Tạo tài khoản nhân viên.
        /// </summary>
        [HttpPost("create-employee")]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
        {
            var emp = await _userService.CreateEmployeeAsync(dto);
            return Ok(emp);
        }

        // Cập nhật quyền người dùng
        /// <summary>
        /// Cập nhật quyền cho người dùng.
        /// </summary>
        [HttpPut("{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromQuery] string newRole)
        {
            var success = await _userService.UpdateUserRoleAsync(userId, newRole);
            if (!success) return NotFound();
            return Ok(new { message = "Cập nhật quyền thành công" });
        }

        [HttpPut("users/{userId}/toggle-active")]
        public async Task<IActionResult> ToggleUserActiveStatus(int userId, [FromQuery] bool active)
        {
            var success = await _userService.ToggleUserActiveStatusAsync(userId, active);

            if (!success)
                return NotFound(new { message = "Không tìm thấy người dùng với ID này." });

            return Ok(new
            {
                message = active 
                    ? "Tài khoản đã được mở khóa thành công." 
                    : "Tài khoản đã bị khóa thành công."
            });
        }
        // DELETE: api/admin/users/5
        // [HttpDelete("{userId}")]
        // public async Task<IActionResult> DeleteUser(int userId)
        // {
        //     var user = await _userService.GetByIdAsync(userId);
        //     if (user == null) return NotFound();

        //     // Soft delete nếu muốn
        //     // user.IsDeleted = true;
        //     // await _context.SaveChangesAsync();

        //     return Ok(new { message = "Xóa user thành công (chưa làm soft delete)" });
        // }
    }
}
