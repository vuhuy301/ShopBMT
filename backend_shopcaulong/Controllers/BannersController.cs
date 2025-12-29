using Microsoft.AspNetCore.Mvc;
using backend_shopcaulong.DTOs.Banner;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend_shopcaulong.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BannersController : ControllerBase
    {
        private readonly IBannerService _service;

        public BannersController(IBannerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var banner = await _service.GetByIdAsync(id);
            return banner == null
                ? NotFound(new { message = "Không tìm thấy banner" })
                : Ok(banner);
        }

        //  CREATE
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateBannerDto dto)
        {
            var result = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                new
                {
                    message = "Thêm banner thành công",
                    data = result
                }
            );
        }

        //  UPDATE
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateBannerDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);

            if (!success)
                return NotFound(new { message = "Không tìm thấy banner để cập nhật" });

            return Ok(new { message = "Cập nhật banner thành công" });
        }

        // DELETE
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Không tìm thấy banner để xoá" });

            return Ok(new { message = "Xoá banner thành công" });
        }
    }

}