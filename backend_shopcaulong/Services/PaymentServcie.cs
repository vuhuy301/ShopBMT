using backend_shopcaulong.DTOs.Common;
using backend_shopcaulong.DTOs.Payment;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ShopDbContext _context;

        public PaymentService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<PaymentDto>> GetPaymentsPagedAsync(GetPaymentsRequestDto request)
        {
            // Validate phân trang
            if (request.PageSize > 100) request.PageSize = 100;
            if (request.PageSize < 1) request.PageSize = 10;
            if (request.PageNumber < 1) request.PageNumber = 1;

            var query = _context.Payments
                .Include(p => p.Order)
                .AsQueryable();

            // Lọc theo Status
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                var statusTrim = request.Status.Trim();
                query = query.Where(p => p.Status == statusTrim);
            }

            // Lọc theo Provider
            if (!string.IsNullOrWhiteSpace(request.Provider))
            {
                var providerTrim = request.Provider.Trim();
                query = query.Where(p => p.Provider == providerTrim);
            }

            // Lọc theo OrderId
            if (request.OrderId.HasValue)
            {
                query = query.Where(p => p.OrderId == request.OrderId.Value);
            }

            // Lọc theo khoảng ngày (dựa vào PaidAt nếu có, hoặc CreatedAt nếu chưa thanh toán)
            if (request.FromDate.HasValue)
            {
                var from = request.FromDate.Value.Date;
                query = query.Where(p => p.PaidAt >= from || (p.PaidAt == null && p.CreatedAt >= from));
            }

            if (request.ToDate.HasValue)
            {
                var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1); // cuối ngày
                query = query.Where(p => p.PaidAt <= to || (p.PaidAt == null && p.CreatedAt <= to));
            }

            // Đếm tổng
            int totalItems = await query.CountAsync();

            // Tính tổng trang
            int totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            // Lấy dữ liệu phân trang
            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    Provider = p.Provider,
                    Amount = p.Amount,
                    TransactionCode = p.TransactionCode,
                    Status = p.Status,
                    PaidAt = p.PaidAt,
                    CreatedAt = p.CreatedAt,
                    RawResponse = p.RawResponse
                })
                .ToListAsync();

            return new PagedResultDto<PaymentDto>
            {
                Page = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                Items = payments
            };
        }

        public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null) return null;

            return new PaymentDto
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                Provider = payment.Provider,
                Amount = payment.Amount,
                TransactionCode = payment.TransactionCode,
                Status = payment.Status,
                PaidAt = payment.PaidAt,
                CreatedAt = payment.CreatedAt,
                RawResponse = payment.RawResponse,
            };
        }
    }
}