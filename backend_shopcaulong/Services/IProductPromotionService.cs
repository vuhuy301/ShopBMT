using backend_shopcaulong.Models;

namespace backend_shopcaulong.Services
{
    public interface IProductPromotionService
    {
        Task AssignPromotionsAsync(int productId, List<int> promotionIds);
        Task<List<Promotion>> GetPromotionsByProductAsync(int productId);
    }
}
