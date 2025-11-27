using backend_shopcaulong.DTOs.Brand;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    // [Authorize(Roles = "Admin")]
    public class AdminBrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public AdminBrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        // Tạo brand
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandCreateDto dto)
        {
            var brand = await _brandService.CreateAsync(dto);
            return Ok(brand);
        }

        // Cập nhật brand
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandUpdateDto dto)
        {
            var brand = await _brandService.UpdateAsync(id, dto);
            if (brand == null) return NotFound();
            return Ok(brand);
        }

        // Xóa brand
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _brandService.DeleteAsync(id);
            if (!success) return BadRequest("Brand còn sản phẩm hoặc không tồn tại.");
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
