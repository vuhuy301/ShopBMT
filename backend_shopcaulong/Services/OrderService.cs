using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;
using backend_shopcaulong.DTOs.Order;

namespace backend_shopcaulong.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopDbContext _context;

        public OrderService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, int? userId = null)
        {
            if (request.Items == null || !request.Items.Any())
                throw new Exception("Giỏ hàng trống.");

            decimal totalAmount = request.Items.Sum(i => i.Price * i.Quantity);

            var order = new Order
            {
                UserId = userId,
                CustomerName = string.IsNullOrWhiteSpace(request.Name) ? "Khách lẻ" : request.Name.Trim(),
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                Status = "Pending",
                PaymentMethod = request.PaymentMethod.ToLower() == "cod" ? "COD" : "Bank",
                ShippingAddress = request.Address.Trim(),
                Phone = request.Phone.Trim(),
                Note = request.Note?.Trim()
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để có Order.Id

                foreach (var item in request.Items)
                {
                    var sizeVariant = await _context.ProductSizeVariants
                        .Include(sv => sv.ColorVariant)
                            .ThenInclude(cv => cv!.Product)
                        .FirstOrDefaultAsync(sv =>
                            sv.Id == item.SizeVariantId &&
                            sv.ColorVariantId == item.ColorVariantId);

                    if (sizeVariant == null)
                        throw new Exception($"Không tìm thấy biến thể size ID {item.SizeVariantId} cho màu ID {item.ColorVariantId}.");

                    string productName = sizeVariant.ColorVariant?.Product?.Name ?? "Sản phẩm";
                    string colorName = sizeVariant.ColorVariant?.Color ?? "N/A";
                    string sizeName = sizeVariant.Size;

                    if (sizeVariant.Stock < item.Quantity)
                        throw new Exception($"Sản phẩm \"{productName}\" màu {colorName} size {sizeName} chỉ còn {sizeVariant.Stock} sản phẩm (bạn chọn {item.Quantity}).");

                    // Trừ tồn kho
                    sizeVariant.Stock -= item.Quantity;

                    // Lưu chi tiết đơn hàng
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ColorVariantId = item.ColorVariantId,
                        SizeVariantId = item.SizeVariantId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateOrderResponse
                {
                    OrderId = order.Id,
                    TotalAmount = totalAmount
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<OrderDto>> GetAllOrdersAsync(GetOrdersRequest request)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                    .ThenInclude(od => od.ColorVariant)
                .Include(o => o.Items)
                    .ThenInclude(od => od.SizeVariant)
                .AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(o => o.Status == request.Status);

            if (request.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

            // Paging
            query = query.OrderByDescending(o => o.CreatedAt)
                         .Skip((request.Page - 1) * request.PageSize)
                         .Take(request.PageSize);

            var orders = await query.ToListAsync();

            return orders.Select(o => MapToOrderDto(o)).ToList();
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(int userId, GetOrdersRequest request)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId) // Chỉ orders của user này
                .Include(o => o.Items)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                    .ThenInclude(od => od.ColorVariant)
                .Include(o => o.Items)
                    .ThenInclude(od => od.SizeVariant)
                .AsQueryable();

            // Filter tương tự
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(o => o.Status == request.Status);

            if (request.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= request.ToDate.Value);

            // Paging
            query = query.OrderByDescending(o => o.CreatedAt)
                         .Skip((request.Page - 1) * request.PageSize)
                         .Take(request.PageSize);

            var orders = await query.ToListAsync();

            return orders.Select(o => MapToOrderDto(o)).ToList();
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                ShippingAddress = order.ShippingAddress,
                Phone = order.Phone,
                Note = order.Note,
                Items = order.Items.Select(od => new OrderDetailDto
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name ?? "N/A",
                    ColorVariantId = od.ColorVariantId,
                    Color = od.ColorVariant?.Color ?? "N/A",
                    SizeVariantId = od.SizeVariantId,
                    Size = od.SizeVariant?.Size ?? "N/A",
                    Quantity = od.Quantity,
                    Price = od.Price
                }).ToList()
            };
        }
    }
}