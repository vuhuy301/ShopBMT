

// backend_shopcaulong/Services/OrderService.cs
using AutoMapper;
using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public OrderService(ShopDbContext db, IEmailSender emailSender, IMapper mapper)
        {
            _db = db;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(od => od.Product)
                .Include(o => o.Items).ThenInclude(od => od.Variant) // BẮT BUỘC PHẢI CÓ 2 DÒNG NÀY RIÊNG BIỆT!
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(od => od.Product)
                .Include(o => o.Items).ThenInclude(od => od.Variant) // QUAN TRỌNG NHẤT!
                .FirstOrDefaultAsync(o => o.Id == orderId);

            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(int userId)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(od => od.Product)
                .Include(o => o.Items).ThenInclude(od => od.Variant) // PHẢI CÓ!
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdForUserAsync(int orderId, int userId)
        {
            var order = await _db.Orders
                .Where(o => o.Id == orderId && o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(od => od.Product)
                .Include(o => o.Items).ThenInclude(od => od.Variant) // ĐÂY LÀ CHÌA KHÓA!
                .FirstOrDefaultAsync();

            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        // Các hàm còn lại giữ nguyên (Approve, Cancel, UpdateStatus...)
        public async Task<bool> ApproveOrderAsync(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null || order.Status != "Pending") return false;
            order.Status = "Confirmed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
        var order = await _db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Items).ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null || order.Status == "Cancelled") return false;

        foreach (var detail in order.Items)
        {
            if (detail.VariantId != null && detail.Variant != null)
            {
                detail.Variant.Stock += detail.Quantity;
                detail.Product.Stock += detail.Quantity;
            }
            else
            {
                detail.Product.Stock += detail.Quantity;
            }
        }

        order.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string newStatus)
        {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Items) // Dù không dùng ở đây nhưng giữ cho chắc
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return false;

        var oldStatus = order.Status;
        order.Status = newStatus;
        await _db.SaveChangesAsync();

        if (oldStatus != newStatus && order.User?.Email != null)
        {
            var name = string.IsNullOrEmpty(order.User.FullName) ? "Quý khách" : order.User.FullName;
            var total = order.Items?.Any() == true 
                ? order.Items.Sum(i => i.Price * i.Quantity) 
                : 0m;

            _ = Task.Run(async () =>
            {
                await _emailSender.SendOrderStatusEmailAsync(
                    order.User.Email, name, order.Id, newStatus, total);
            });
        }
        return true;
        }
    }
}