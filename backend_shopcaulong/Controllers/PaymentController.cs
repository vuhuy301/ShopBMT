using backend_shopcaulong.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using backend_shopcaulong.DTOs.Payment;
using backend_shopcaulong.Services;

namespace backend_shopcaulong.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ShopDbContext _shopDbContext;
        private readonly IEmailSender _emailSender;

        public PaymentController(ShopDbContext shopDbContext, IEmailSender emailSender)
        {
            _shopDbContext = shopDbContext;
            _emailSender = emailSender;
        }

        [HttpPost("sepay/webhook")]
        public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookDto dto)
        {
            Console.WriteLine("🔥 Webhook received");
            Console.WriteLine(JsonSerializer.Serialize(dto));
            // ⚠️ Webhook: tuyệt đối KHÔNG throw / KHÔNG return 4xx
            if (dto == null)
            {
                Console.WriteLine("❌ dto null");
                return Ok();
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                Console.WriteLine("❌ Content null");
                return Ok();
            }

            Console.WriteLine($"👉 Content = {dto.Content}");

            if (!int.TryParse(dto.Content.Replace("SP", ""), out int orderId))
            {
                Console.WriteLine("❌ Parse orderId fail");
                return Ok();
            }

            Console.WriteLine($"✅ OrderId = {orderId}");

            // 2️⃣ Chống webhook trùng (idempotent)
            var existedPayment = await _shopDbContext.Payments
                .AnyAsync(p => p.TransactionCode == dto.ReferenceCode);

            if (existedPayment)
                return Ok();

            // 3️⃣ Lấy Order
            var order = await _shopDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return Ok();

            // 4️⃣ Tạo Payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                TransactionCode = dto.ReferenceCode,
                Amount = dto.TransferAmount,
                Status = "Success", // SePay chỉ bắn khi tiền VÀO
                PaidAt = DateTime.Now,
                RawResponse = JsonSerializer.Serialize(dto)
            };

            // 5️⃣ Update Order
            order.Status = "Paid";

            _shopDbContext.Payments.Add(payment);
            await _shopDbContext.SaveChangesAsync();

            // Gửi email thông báo thanh toán thành công
            if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
            {
                try
                {
                    await _emailSender.SendOrderStatusEmailAsync(
                        order.CustomerEmail,
                        order.CustomerName,
                        order.Id,
                        order.Status,      // "Paid"
                        order.TotalAmount
                    );
                    Console.WriteLine($"✅ Email sent to {order.CustomerEmail}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to send email: {ex.Message}");
                    // Không throw, chỉ log
                }
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
                Status = "Pending"
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

    }
}

