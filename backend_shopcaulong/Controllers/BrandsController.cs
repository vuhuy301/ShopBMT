using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    /// <summary>
    /// API xem thông tin thương hiệu.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
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
        /// Lấy chi tiết thương hiệu theo ID.
        /// </summary>
        /// <param name="id">ID thương hiệu.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null) return NotFound();
            return Ok(brand);
        }
    }
}
