using backend_shopcaulong.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using backend_shopcaulong.DTOs.Payment;
using backend_shopcaulong.Services;
using backend_shopcaulong.DTOs.Common;
using Microsoft.AspNetCore.Authorization;

namespace backend_shopcaulong.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ShopDbContext _shopDbContext;
        private readonly IEmailSender _emailSender;
        private readonly IPaymentService _paymentService;

        public PaymentController(ShopDbContext shopDbContext, IEmailSender emailSender, IPaymentService paymentService)
        {
            _shopDbContext = shopDbContext;
            _emailSender = emailSender;
            _paymentService = paymentService;
        }

        [HttpPost("sepay/webhook")]
        public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookDto dto)
        {
            // ❗ webhook luôn return 200
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                return Ok();

            // SP123 → 123
            if (!int.TryParse(dto.Content.Replace("SP", ""), out int orderId))
                return Ok();

            var order = await _shopDbContext.Orders.FindAsync(orderId);
            if (order == null)
                return Ok();

            // 🔒 Tìm payment đã tạo
            var payment = await _shopDbContext.Payments
                .FirstOrDefaultAsync(p =>
                    p.OrderId == orderId &&
                    p.Status == "Chờ thanh toán"
                );

            if (payment == null)
                return Ok();

            // 🔒 Chống webhook trùng
            if (payment.Status == "Thành công")
                return Ok();

            // ✅ Update payment
            payment.Status = "Thành công";
            payment.PaidAt = DateTime.Now;
            payment.RawResponse = JsonSerializer.Serialize(dto);

            // ✅ Update order
            order.Status = "Đã thanh toán";

            await _shopDbContext.SaveChangesAsync();

            // 📧 Gửi email
            if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
            {
                await _emailSender.SendOrderStatusEmailAsync(
                    order.CustomerEmail,
                    order.CustomerName,
                    order.Id,
                    order.Status,
                    order.TotalAmount
                );
            }

            return Ok();
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest req)
        {
            var order = await _shopDbContext.Orders.FindAsync(req.OrderId);
            if (order == null) return NotFound();

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Provider = "SePay",
                Status = "Chờ thanh toán"    
            };

            _shopDbContext.Payments.Add(payment);
            await _shopDbContext.SaveChangesAsync();

            payment.TransactionCode = $"SP{order.Id}";
            await _shopDbContext.SaveChangesAsync();

            return Ok(new
            {
                payment.Id,
                payment.TransactionCode,
                payment.Amount
            });
        }
        [HttpGet]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResultDto<PaymentDto>>> GetPayments(
            [FromQuery] GetPaymentsRequestDto request)
        {
            var result = await _paymentService.GetPaymentsPagedAsync(request);
            return Ok(result);
        }

        // GET: api/admin/payments/5
        [HttpGet("/admin/{id}")]
        // [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PaymentDto>> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound(new { message = "Không tìm thấy thanh toán." });

            return Ok(payment);
        }
    }
}

