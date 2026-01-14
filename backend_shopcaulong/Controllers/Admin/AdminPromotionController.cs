using backend_shopcaulong.DTOs.Promotion;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminPromotionController : Controller
    {
        private readonly IPromotionService _promotionService;

        public AdminPromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        // GET: api/promotion
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _promotionService.GetAllAsync());
        }

        // GET: api/promotion/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var promotion = await _promotionService.GetByIdAsync(id);
            if (promotion == null) return NotFound();
            return Ok(promotion);
        }

        // POST: api/promotion
        [HttpPost]
        public async Task<IActionResult> Create(PromotionCreateDto dto)
        {
            try
            {
                var promotion = await _promotionService.CreateAsync(dto);
                return Ok(promotion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/promotion/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PromotionUpdateDto dto)
        {
            try
            {
                var promotion = await _promotionService.UpdateAsync(id, dto);
                if (promotion == null) return NotFound();
                return Ok(promotion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/promotion/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _promotionService.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok("Xóa ưu đãi thành công");
        }
    }
}
