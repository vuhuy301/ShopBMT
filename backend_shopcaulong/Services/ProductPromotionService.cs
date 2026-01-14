using backend_shopcaulong.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Services
{
    public class ProductPromotionService : IProductPromotionService
    {
        private readonly ShopDbContext _context;

        public ProductPromotionService(ShopDbContext context)
        {
            _context = context;
        }

        public async Task AssignPromotionsAsync(int productId, List<int> promotionIds)
        {
            var oldItems = await _context.ProductPromotions
                .Where(x => x.ProductId == productId)
                .ToListAsync();

            if (oldItems.Any())
                _context.ProductPromotions.RemoveRange(oldItems);

            if (promotionIds != null && promotionIds.Any())
            {
                var newItems = promotionIds
                    .Distinct()
                    .Select(promotionId => new ProductPromotion
                    {
                        ProductId = productId,
                        PromotionId = promotionId
                    });

                await _context.ProductPromotions.AddRangeAsync(newItems);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Promotion>> GetPromotionsByProductAsync(int productId)
        {
            return await _context.ProductPromotions
                .Where(x => x.ProductId == productId)
                .Include(x => x.Promotion)
                .Select(x => x.Promotion)
                .ToListAsync();
        }
    }
}
