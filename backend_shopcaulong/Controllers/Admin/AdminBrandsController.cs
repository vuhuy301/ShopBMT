using backend_shopcaulong.DTOs.Brand;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
/// <summary>
/// Quản lý thương hiệu (Brand) cho Admin.
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminBrandsController : ControllerBase
{
    private readonly IBrandService _brandService;

    public AdminBrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }
     /// <summary>
        /// Lấy danh sách tất cả thương hiệu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _brandService.GetAllAsync();
            return Ok(brands);
        }
    /// <summary>
    /// Tạo thương hiệu mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BrandCreateDto dto)
    {
        var brand = await _brandService.CreateAsync(dto);
        return Ok(brand);
    }

    /// <summary>
    /// Cập nhật thương hiệu.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] BrandUpdateDto dto)
    {
        var brand = await _brandService.UpdateAsync(id, dto);
        if (brand == null) return NotFound();
        return Ok(brand);
    }

        /// <summary>
        /// Xóa thương hiệu.
        /// </summary>
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id, [FromQuery] bool isActive)
        {
            var success = await _brandService.ToggleBrandActiveAsync(id, isActive);
            if (!success) return NotFound("Brand không tồn tại.");
            return Ok(new { message = isActive ? "Đã hiển thị" : "Đã ẩn" });
        }
    }

}
