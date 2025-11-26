namespace backend_shopcaulong.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendOrderStatusEmailAsync(string toEmail, string customerName, int orderId, string newStatus, decimal totalAmount);
    }
}