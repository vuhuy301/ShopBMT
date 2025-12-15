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
    }
}