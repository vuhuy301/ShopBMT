using backend_shopcaulong.DTOs.Category;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    /// <summary>
    /// API quản lý danh mục sản phẩm.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
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
    }
}
