using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend_shopcaulong.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    // [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _orderService.ApproveOrderAsync(id);
            if (!result) return BadRequest("Chỉ duyệt đơn trạng thái Pending");
            return Ok(new { message = "Đơn hàng đã được duyệt" });
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            if (!result) return BadRequest("Đơn hàng không tồn tại hoặc đã bị hủy");
            return Ok(new { message = "Đơn hàng đã được hủy và tồn kho hoàn trả" });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] string newStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, newStatus);
            if (!result) return NotFound();
            return Ok(new { message = $"Trạng thái đơn hàng đã được cập nhật thành '{newStatus}'" });
        }
    }
}
