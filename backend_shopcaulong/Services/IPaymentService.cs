using backend_shopcaulong.DTOs.Common;
using backend_shopcaulong.DTOs.Payment;

namespace backend_shopcaulong.Services
{
    public interface IPaymentService
    {
        Task<PagedResultDto<PaymentDto>> GetPaymentsPagedAsync(GetPaymentsRequestDto request);
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        // Các hàm khác nếu cần: UpdateStatus, v.v.
    }
}