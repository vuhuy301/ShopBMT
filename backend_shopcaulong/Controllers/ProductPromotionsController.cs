using backend_shopcaulong.DTOs.Promotion;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class ProductPromotionsController : ControllerBase
    {
        private readonly IProductPromotionService _service;

        public ProductPromotionsController(IProductPromotionService service)
        {
            _service = service;
        }
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> AssignPromotions(
            [FromBody] AssignProductPromotionDto dto)
        {
            if (dto.ProductId <= 0)
                return BadRequest("ProductId không hợp lệ");

            await _service.AssignPromotionsAsync(dto.ProductId, dto.PromotionIds);

            return Ok(new
            {
                message = "Gán ưu đãi cho sản phẩm thành công"
            });
        }

        /// <summary>
        /// Lấy danh sách ưu đãi của 1 sản phẩm
        /// Dùng cho: edit sản phẩm, hiển thị admin
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetPromotionsByProduct(int productId)
        {
            if (productId <= 0)
                return BadRequest("ProductId không hợp lệ");

            var promotions = await _service.GetPromotionsByProductAsync(productId);

            return Ok(promotions);
        }
    }
}
