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
          /// <summary>
        /// Lấy tất cả danh mục.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            return Ok(await _categoryService.GetAllAsync());
        }

        /// <summary>
        /// Lấy chi tiết danh mục theo ID.
        /// </summary>
        /// <param name="id">ID danh mục.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            return cat == null ? NotFound() : Ok(cat);
        }
        /// <summary>
        /// Tạo danh mục sản phẩm
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CategoryCreateUpdateDto dto)
        {
            var cat = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = cat.Id }, cat);
        }

        // Cập nhật category
        /// <summary>
        /// Cập nhật mục sản phẩm
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(int id, CategoryCreateUpdateDto dto)
        {
            var cat = await _categoryService.UpdateAsync(id, dto);
            return cat == null ? NotFound() : Ok(cat);
        }

        // Xóa category
        /// <summary>
        /// Xóa mục sản phẩm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
