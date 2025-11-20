using backend_shopcaulong.DTOs.Category;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet] public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll() => Ok(await _categoryService.GetAllAsync());
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            return cat == null ? NotFound() : Ok(cat);
        }
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CategoryCreateUpdateDto dto)
        {
            var cat = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = cat.Id }, cat);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> Update(int id, CategoryCreateUpdateDto dto)
        {
            var cat = await _categoryService.UpdateAsync(id, dto);
            return cat == null ? NotFound() : Ok(cat);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _categoryService.DeleteAsync(id) ? NoContent() : NotFound();
        }
    }
}
