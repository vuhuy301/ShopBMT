// Services/EmailSender.cs
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace backend_shopcaulong.Services
{

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
            var senderName = _config["EmailSettings:SenderName"] ?? "BadmintonShop";
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var username = _config["EmailSettings:Username"];
            var password = _config["EmailSettings:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        //Hàm chuyên dụng: Gửi thông báo trạng thái đơn hàng
        public async Task SendOrderStatusEmailAsync(string toEmail, string customerName, int orderId, string newStatus, decimal totalAmount)
        {
            var statusText = newStatus switch
            {
                "Pending" => "Chờ xác nhận",
                "Confirmed" => "Đã xác nhận",
                "Preparing" => "Đang chuẩn bị hàng",
                "Shipping" => "Đang giao hàng",
                "Delivered" => "Đã giao thành công",
                "Cancelled" => "Đã hủy",
                "Returned" => "Đã hoàn trả",
                "Paid" => "Thanh toán thành công",
                _ => newStatus
            };

            var statusColor = newStatus switch
            {
                "Delivered" => "#52c41a",
                "Cancelled" or "Returned" => "#ff4d4f",
                "Confirmed" or "Shipping" => "#fa8c16",
                _ => "#1890ff"
            };

            var subject = $"[CẬP NHẬT] Đơn hàng #{orderId} - {statusText}";

            var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e8e8e8; border-radius: 10px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #ff7e5f, #feb47b); padding: 20px; text-align: center; color: white;'>
                    <h1>Shop Cầu Lông</h1>
                    <p>Chuyên vợt Yonex - Victor - Lining chính hãng</p>
                </div>
                <div style='padding: 30px; background: #fafafa;'>
                    <h2>Xin chào <strong>{customerName}</strong>,</h2>
                    <p>Cảm ơn bạn đã tin tưởng mua sắm tại Shop Cầu Lông!</p>
                    
                    <div style='background: white; padding: 20px; border-radius: 8px; border-left: 5px solid {statusColor}; margin: 20px 0;'>
                        <h3 style='margin: 0; color: #333;'>Trạng thái đơn hàng của bạn đã thay đổi:</h3>
                        <h1 style='margin: 10px 0; color: {statusColor}; font-size: 28px;'><strong>{statusText}</strong></h1>
                        <p><strong>Mã đơn hàng:</strong> #{orderId}</p>
                        <p><strong>Tổng tiền:</strong> {totalAmount:N0}₫</p>
                    </div>

                    <p style='text-align: center;'>
                        <a href='http://localhost:3000/my-order/{orderId}' 
                           style='background: #d4380d; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold;'>
                            XEM CHI TIẾT ĐƠN HÀNG
                        </a>
                    </p>

                    <hr style='border: 1px dashed #ddd; margin: 30px 0;'>
                    <p>Nếu bạn có thắc mắc, vui lòng liên hệ:</p>
                    <p><strong>Hotline:</strong> 0909.123.456<br>
                       <strong>Email:</strong> support@shopcaulong.com</p>
                </div>
                <div style='background: #333; color: #aaa; padding: 15px; text-align: center; font-size: 12px;'>
                    © 2025 Shop Cầu Lông - Chuyên đồ cầu lông chính hãng
                </div>
            </div>";

            await SendEmailAsync(toEmail, subject, htmlMessage);
        }
    }
}