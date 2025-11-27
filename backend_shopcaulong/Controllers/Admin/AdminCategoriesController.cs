using backend_shopcaulong.DTOs.Category;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    // [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Tạo category
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CategoryCreateUpdateDto dto)
        {
            var cat = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = cat.Id }, cat);
        }

        // Cập nhật category
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(int id, CategoryCreateUpdateDto dto)
        {
            var cat = await _categoryService.UpdateAsync(id, dto);
            return cat == null ? NotFound() : Ok(cat);
        }

        // Xóa category
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
