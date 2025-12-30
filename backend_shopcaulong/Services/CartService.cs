using backend_shopcaulong.DTOs.Cart;
using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class CartService : ICartService
    {
        private readonly ShopDbContext _context;

        public CartService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<StockCheckResult> CheckStockAsync(List<CartItemDto> cartItems)
        {
            var result = new StockCheckResult();
            bool allEnough = true;

            foreach (var item in cartItems)
            {
                var sizeVariant = await _context.ProductSizeVariants
                    .Include(sv => sv.ColorVariant)
                        .ThenInclude(cv => cv.Product)
                    .FirstOrDefaultAsync(sv =>
                        sv.Id == item.SizeVariantId &&
                        sv.ColorVariantId == item.ColorVariantId &&
                        sv.ColorVariant.ProductId == item.ProductId
                    );

                if (sizeVariant == null)
                {
                    allEnough = false;
                    result.Items.Add(new StockCheckItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Requested = item.Quantity,
                        Available = 0,
                        Message = "Biến thể màu/size không tồn tại"
                    });
                    continue;
                }

                if (sizeVariant.Stock < item.Quantity)
                {
                    allEnough = false;
                    result.Items.Add(new StockCheckItem
                    {
                        ProductId = item.ProductId,
                        ProductName = sizeVariant.ColorVariant.Product.Name,
                        Requested = item.Quantity,
                        Available = sizeVariant.Stock,
                        Message = "Không đủ số lượng tồn kho"
                    });
                }
            }

            result.Ok = allEnough;
            return result;
        }
    }
}

