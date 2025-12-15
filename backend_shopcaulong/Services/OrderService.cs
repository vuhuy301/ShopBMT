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
                CustomerName = request.Name.Trim(),
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
                await _context.SaveChangesAsync(); // Lưu để lấy Order.Id

                foreach (var item in request.Items)
                {
                    // Tìm SizeVariant chính xác (phải thuộc ColorVariant đúng)
                    var sizeVariant = await _context.ProductSizeVariants
                        .FirstOrDefaultAsync(sv =>
                            sv.Id == item.SizeVariantId &&
                            sv.ColorVariantId == item.ColorVariantId);

                    if (sizeVariant == null)
                    {
                        var color = await _context.ProductColorVariants
                            .Where(c => c.Id == item.ColorVariantId)
                            .Select(c => c.Color)
                            .FirstOrDefaultAsync();

                        throw new Exception($"Không tìm thấy biến thể size ID {item.SizeVariantId} cho màu {color ?? item.ColorVariantId.ToString()}.");
                    }

                    // Kiểm tra tồn kho
                    if (sizeVariant.Stock < item.Quantity)
                    {
                        var productName = await _context.Products
                            .Where(p => p.Id == item.ProductId)
                            .Select(p => p.Name)
                            .FirstOrDefaultAsync();

                        throw new Exception($"Sản phẩm \"{productName}\" " +
                                            $"màu {sizeVariant.ColorVariant.Color} " +
                                            $"size {sizeVariant.Size} " +
                                            $"chỉ còn {sizeVariant.Stock} sản phẩm (bạn chọn {item.Quantity}).");
                    }

                    // Trừ tồn kho
                    sizeVariant.Stock -= item.Quantity;

                    // Tạo chi tiết đơn hàng
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ColorVariantId = item.ColorVariantId,
                        SizeVariantId = item.SizeVariantId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };

                    _context.OrderDetails.Add(orderDetail);
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