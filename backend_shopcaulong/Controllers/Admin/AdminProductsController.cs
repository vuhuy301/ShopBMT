using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
   
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IAiSyncService _aiSyncService;

        public AdminProductsController(IProductService productService, IAiSyncService aiSyncService)
        {
            _productService = productService;
            _aiSyncService = aiSyncService;
        }
        // Thêm vào AdminProductsController
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync(); // dùng chung service
            return Ok(products);
        }

        // Tạo sản phẩm
        /// <summary>
        /// Tạo mới một sản phẩm.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromForm] ProductCreateDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = product.Id }, product);
        }

        // Cập nhật sản phẩm
        /// <summary>
        /// Cập nhật thông tin sản phẩm.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> Update(int id,[FromForm]  ProductUpdateDto dto)
        {
            var dtoJson = JsonSerializer.Serialize(dto, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            Console.WriteLine("Received DTO from FE:");
            Console.WriteLine(dtoJson);
            var product = await _productService.UpdateAsync(id, dto);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // Xóa sản phẩm
        /// <summary>
        /// Xóa sản phẩm theo ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        [HttpPost("reindex-ai")]
        public async Task<IActionResult> ReindexAi()
        {
            var products = await _productService.GetAllAsync(); // Lấy hết từ DB
            await _aiSyncService.RebuildAllAsync(products);
            return Ok("Đã gửi yêu cầu rebuild AI thành công! Vui lòng đợi 5-30s tùy số lượng sản phẩm.");
        }

    }
}
