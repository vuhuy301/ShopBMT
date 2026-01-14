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
        private readonly IProductPromotionService _productPromotionService;

        public AdminProductsController(IProductService productService, IProductPromotionService productPromotionService)
        {
            _productService = productService;
            _productPromotionService = productPromotionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _productService.GetAllAsync(); 
            return Ok(products);
        }
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromForm] ProductCreateDto dto)
        {
            var product = await _productService.CreateAsync(dto);

            // 2. Gán ưu đãi (NẾU CÓ)
            if (dto.PromotionIds != null && dto.PromotionIds.Any())
            {
                await _productPromotionService.AssignPromotionsAsync(
                    product.Id,
                    dto.PromotionIds
                );
            }

            return Ok(new
            {
                message = "Tạo sản phẩm thành công",
                product.Id
            });
        }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        
    }
}
