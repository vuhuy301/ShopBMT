using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Get tất cả sản phẩm
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        // Get sản phẩm theo id
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // Lấy sản phẩm có phân trang
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetPagedAsync(page, pageSize);
            return Ok(result);
        }

        // API lọc sản phẩm
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

        // Top sản phẩm mới theo category
        [HttpGet("top-new/category/{categoryId}")]
        public async Task<IActionResult> GetTopNewByCategory(int categoryId)
        {
            var products = await _productService.GetTopNewByCategoryAsync(categoryId);
            return Ok(products);
        }
    }
}
