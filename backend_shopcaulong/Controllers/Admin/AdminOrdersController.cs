using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace backend_shopcaulong.Controllers.Admin
{

namespace backend_shopcaulong.Controllers.Admin
{
    /// <summary>
    /// API quản lý đơn hàng dành cho Admin và Nhân viên.
    /// </summary>
    [ApiController]
    [Route("api/admin/orders")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy danh sách tất cả đơn hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng theo ID.
        /// </summary>
        /// <param name="id">ID đơn hàng.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return order == null ? NotFound("Không tìm thấy đơn hàng") : Ok(order);
        }

        /// <summary>
        /// Duyệt đơn hàng.
        /// </summary>
        /// <param name="id">ID đơn hàng.</param>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _orderService.ApproveOrderAsync(id);
            return result
                ? Ok(new { message = "Đơn hàng đã được duyệt thành công và khách hàng đã nhận email thông báo." })
                : BadRequest("Chỉ có thể duyệt đơn hàng đang ở trạng thái 'Pending'");
        }

        /// <summary>
        /// Hủy đơn hàng.
        /// </summary>
        /// <param name="id">ID đơn hàng.</param>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            return result
                ? Ok(new { message = "Đơn hàng đã được hủy thành công. Tồn kho đã được hoàn trả và khách hàng đã nhận email thông báo." })
                : BadRequest("Đơn hàng không tồn tại hoặc đã bị hủy trước đó");
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng.
        /// </summary>
        /// <param name="id">ID đơn hàng.</param>
        /// <param name="dto">Trạng thái mới.</param>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewStatus))
                return BadRequest("Trạng thái mới không được để trống");

            var result = await _orderService.UpdateOrderStatusAsync(id, dto.NewStatus);

            if (!result)
                return NotFound("Không tìm thấy đơn hàng");

            // Danh sách trạng thái hợp lệ (tùy bạn mở rộng)
            var validStatuses = new[] { "Pending", "Confirmed", "Preparing", "Shipping", "Delivered", "Cancelled", "Returned" };
            var displayStatus = validStatuses.Contains(dto.NewStatus)
                ? dto.NewStatus switch
                {
                    "Confirmed" => "Đã xác nhận",
                    "Preparing" => "Đang chuẩn bị hàng",
                    "Shipping" => "Đang giao hàng",
                    "Delivered" => "Đã giao thành công",
                    "Cancelled" => "Đã hủy",
                    "Returned" => "Đã hoàn trả",
                    _ => "Chờ xác nhận"
                }
                : dto.NewStatus;

            return Ok(new
            {
                message = $"Trạng thái đơn hàng đã được cập nhật thành công thành '{displayStatus}'.",
                notification = "Khách hàng đã nhận được email thông báo tự động.",
                newStatus = dto.NewStatus
            });
        }
    }

    /// <summary>
    /// Request cập nhật trạng thái đơn hàng.
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// Trạng thái mới của đơn hàng.
        /// </summary>
        public string NewStatus { get; set; } = string.Empty;
    }
}

}