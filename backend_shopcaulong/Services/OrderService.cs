using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;
using backend_shopcaulong.DTOs.Order;
using backend_shopcaulong.DTOs.Notification;

namespace backend_shopcaulong.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopDbContext _context;

        private readonly IEmailSender _emailSender;

        private readonly INotificationService _notificationService;

        public OrderService(ShopDbContext context, IEmailSender emailSender, INotificationService notificationService)
        {
            _context = context;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, int? userId = null)
        {
            if (request.Items == null || !request.Items.Any())
                throw new Exception("Giỏ hàng trống.");

            decimal totalAmount = request.Items.Sum(i => i.Price * i.Quantity);
            User? user = null;

            if (userId.HasValue)
            {
                user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId.Value);
            }
            string customerName =
            !string.IsNullOrWhiteSpace(request.Name)
                ? request.Name.Trim()
                : !string.IsNullOrWhiteSpace(user?.FullName)
                    ? user.FullName
                    : "Quý khách";

            var order = new Order
            {
                UserId = userId,
                CustomerName = customerName,
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

                try
                {
                    // Lấy danh sách User là Admin hoặc Staff
                    var adminStaffUserIds = await _context.Users.Include(u => u.Role)
                        .Where(u => u.Role.Name == "Admin" || u.Role.Name == "Staff")
                        .Select(u => u.Id)
                        .ToListAsync();

                    if (adminStaffUserIds.Any())
                    {
                        var notificationDto = new CreateNotificationDto
                        {
                            Title = "Đơn hàng mới",
                            Message = $"Có đơn hàng mới #{order.Id} từ {customerName} - Tổng: {totalAmount:N0}₫",
                            Type = "NewOrder",
                            ReferenceId = order.Id
                        };

                        // Tạo thông báo cho từng admin/staff
                        foreach (var adminId in adminStaffUserIds)
                        {
                            notificationDto.UserId = adminId;
                            await _notificationService.CreateAsync(notificationDto);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Không làm hỏng đơn hàng nếu gửi thông báo lỗi
                    // Có thể log lại để theo dõi
                    Console.WriteLine($"Lỗi khi gửi thông báo đơn hàng mới: {ex.Message}");
                    // Hoặc dùng ILogger nếu bạn có inject
                }

                // ==================================================================

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
        public async Task<List<OrderDto>> GetOrdersAsync(GetOrdersRequest request)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                    .ThenInclude(od => od.ColorVariant)
                .Include(o => o.Items)
                    .ThenInclude(od => od.SizeVariant)
                .AsQueryable();

            // 🔍 Search theo OrderId
            if (request.OrderId.HasValue)
            {
                query = query.Where(o => o.Id == request.OrderId.Value);
            }

            // 🔍 Search theo Phone
            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                query = query.Where(o => o.Phone == request.Phone.Trim());
            }

            // 🔎 Filter Status
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(o => o.Status == request.Status.Trim());
            }

            // 📅 FromDate
            if (request.FromDate.HasValue && request.FromDate.Value.Year >= 1900)
            {
                query = query.Where(o => o.CreatedAt >= request.FromDate.Value);
            }

            // 📅 ToDate (đến hết ngày)
            if (request.ToDate.HasValue && request.ToDate.Value.Year >= 1900)
            {
                var endOfDay = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= endOfDay);
            }

            // 📌 Sort
            query = query.OrderByDescending(o => o.CreatedAt);

            // 📄 Paging
            query = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var orders = await query.ToListAsync();
            return orders.Select(MapToOrderDto).ToList();
        }


        public async Task<List<OrderDto>> GetMyOrdersAsync(int userId, GetOrdersRequest request)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                .ThenInclude(od => od.ColorVariant)
                .Include(o => o.Items)
                .ThenInclude(od => od.SizeVariant)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(o => o.Status == request.Status);

            if (request.FromDate.HasValue && request.FromDate.Value.Year > 1900)
            {
                query = query.Where(o => o.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue && request.ToDate.Value.Year > 1900)
            {
                var endOfDay = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= endOfDay);
            }

            // Paging
            // query = query.OrderByDescending(o => o.CreatedAt)
            //              .Skip((request.Page - 1) * request.PageSize)
            //              .Take(request.PageSize);

            var orders = await query.ToListAsync();

            return orders.Select(o => MapToOrderDto(o)).ToList();
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(
            int orderId,
            string newStatus,
            int adminUserId)
        {
            var validStatuses = new HashSet<string>
            {
                "Pending", "Paid", "Shipping", "Completed", "Cancelled"
            };

            if (!validStatuses.Contains(newStatus))
                throw new Exception("Trạng thái không hợp lệ.");

            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Items)
                    .ThenInclude(od => od.ColorVariant)
                .Include(o => o.Items)
                    .ThenInclude(od => od.SizeVariant)
                .Include(o => o.User) // ⚠️ BẮT BUỘC: để lấy email
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");

            // ❌ Không cho hủy sai trạng thái
            if (newStatus == "Cancelled" &&
                order.Status != "Pending" &&
                order.Status != "Paid")
            {
                throw new Exception("Chỉ có thể hủy đơn khi Pending hoặc Paid.");
            }

            // 🔁 Hoàn kho khi hủy
            if (newStatus == "Cancelled" &&
                (order.Status == "Pending" || order.Status == "Paid"))
            {
                foreach (var item in order.Items)
                {
                    var sizeVariant = await _context.ProductSizeVariants
                        .FirstOrDefaultAsync(sv =>
                            sv.Id == item.SizeVariantId &&
                            sv.ColorVariantId == item.ColorVariantId);

                    if (sizeVariant != null)
                        sizeVariant.Stock += item.Quantity;
                }
            }

            var oldStatus = order.Status;
            order.Status = newStatus;

            await _context.SaveChangesAsync(); // ✅ LƯU TRƯỚC
            string customerName =
                !string.IsNullOrWhiteSpace(order.CustomerName)
                    ? order.CustomerName
                    : !string.IsNullOrWhiteSpace(order.User?.FullName)
                        ? order.User.FullName
                        : "Quý khách";

            // =========================
            // 📧 GỬI EMAIL THÔNG BÁO
            // =========================
            if (!string.IsNullOrEmpty(order.User?.Email))
            {
                await _emailSender.SendOrderStatusEmailAsync(
                    toEmail: order.User.Email,
                    customerName: customerName,
                    orderId: order.Id,
                    newStatus: newStatus,
                    totalAmount: order.TotalAmount
                );
            }

            return MapToOrderDto(order);
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