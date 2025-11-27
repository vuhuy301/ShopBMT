using backend_shopcaulong.DTOs.Product;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    // [Authorize(Roles = "Admin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Tạo sản phẩm
        /// <summary>
        /// Tạo mới một sản phẩm.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = product.Id }, product);
        }

        // Cập nhật sản phẩm
        /// <summary>
        /// Cập nhật thông tin sản phẩm.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> Update(int id, ProductUpdateDto dto)
        {
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

    }
}
