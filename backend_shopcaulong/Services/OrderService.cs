using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopDbContext _db;

        public OrderService(ShopDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                    .ThenInclude(od => od.Variant)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserFullName = o.User?.FullName ?? "Khách vãng lai",
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                ShippingAddress = o.ShippingAddress,
                Phone = o.Phone,
                Items = o.Items.Select(i => new OrderDetailDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    VariantId = i.VariantId,
                    // Lấy tên biến thể từ màu sắc + size (nếu có)
                    VariantName = i.Variant != null 
                        ? $"{(string.IsNullOrEmpty(i.Variant.Color) ? "" : i.Variant.Color)} {(string.IsNullOrEmpty(i.Variant.Size) ? "" : i.Variant.Size)}".Trim()
                        : "",
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            }).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
        {
            var o = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                    .ThenInclude(od => od.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (o == null) return null;

            return new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                UserFullName = o.User?.FullName ?? "Khách vãng lai",
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                ShippingAddress = o.ShippingAddress,
                Phone = o.Phone,
                Items = o.Items.Select(i => new OrderDetailDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    VariantId = i.VariantId,
                    VariantName = i.Variant != null 
                        ? $"{(string.IsNullOrEmpty(i.Variant.Color) ? "" : i.Variant.Color)} {(string.IsNullOrEmpty(i.Variant.Size) ? "" : i.Variant.Size)}".Trim()
                        : "",
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };
        }

        public async Task<bool> ApproveOrderAsync(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null || order.Status != "Pending")
                return false;

            order.Status = "Confirmed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Variant)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status == "Cancelled")
                return false;

            // Hoàn trả tồn kho
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
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
