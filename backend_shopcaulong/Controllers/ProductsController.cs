using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    /// <summary>
    /// API quản lý sản phẩm.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Lấy tất cả sản phẩm.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        /// <summary>
        /// Lấy sản phẩm theo ID.
        /// </summary>
        /// <param name="id">ID sản phẩm.</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        /// <summary>
        /// Lấy sản phẩm theo trang với phân trang.
        /// </summary>
        /// <param name="page">Số trang, mặc định 1.</param>
        /// <param name="pageSize">Số mục trên trang, mặc định 10.</param>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetPagedAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Lọc sản phẩm theo tiêu chí.
        /// </summary>
        /// <param name="categoryId">ID danh mục (tùy chọn).</param>
        /// <param name="brandId">ID thương hiệu (tùy chọn).</param>
        /// <param name="search">Từ khóa tìm kiếm (tùy chọn).</param>
        /// <param name="sortBy">Sắp xếp (tùy chọn).</param>
        /// <param name="page">Số trang, mặc định 1.</param>
        /// <param name="pageSize">Số mục trên trang, mặc định 10.</param>
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            [FromQuery] int? categoryId,
            [FromQuery] int? brandId,
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetProductsByFilterAsync(
                categoryId,
                brandId,
                search,
                sortBy,
                page,
                pageSize
            );

            return Ok(result);
        }

        /// <summary>
        /// Lấy top sản phẩm mới theo danh mục.
        /// </summary>
        /// <param name="categoryId">ID danh mục.</param>
        [HttpGet("top-new/category/{categoryId}")]
        public async Task<IActionResult> GetTopNewByCategory(int categoryId)
        {
            var products = await _productService.GetTopNewByCategoryAsync(categoryId);
            return Ok(products);
        }
    }
}
